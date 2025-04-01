﻿using System;
using System.Collections.Generic;
using System.Linq;
using Editors.Audio.AudioExplorer;
using Editors.Audio.Storage;
using Serilog;
using Shared.Core.ErrorHandling;
using Shared.GameFormats.Wwise.Enums;
using Shared.GameFormats.Wwise.Hirc;

namespace Editors.Audio.Utility
{
    public abstract class WwiseTreeParserBase
    {
        protected ILogger _logger = Logging.Create<WwiseTreeParserBase>();

        protected Dictionary<AkBkHircType, Action<HircItem, HircTreeItem>> _hircProcessChildMap = [];
        protected readonly IAudioRepository _repository;

        protected readonly bool _showId;
        protected readonly bool _showOwningBnkFile;
        protected readonly bool _filterByBnkName;

        public WwiseTreeParserBase(IAudioRepository repository, bool showId, bool showOwningBnkFile, bool filterByBnkName)
        {
            _repository = repository;
            _showId = showId;
            _showOwningBnkFile = showOwningBnkFile;
            _filterByBnkName = filterByBnkName;
        }

        public HircTreeItem BuildHierarchy(HircItem item)
        {
            var root = new HircTreeItem();
            ProcessHircObject(item, root);
            var actualRoot = root.Children.FirstOrDefault();
            actualRoot.Parent = null;
            root.Children = null;
            return actualRoot;
        }

        public List<HircTreeItem> BuildHierarchyAsFlatList(HircItem item)
        {
            var rootNode = BuildHierarchy(item);

            var flatList = GetHircParents(rootNode);
            //flatList.Reverse();
            return flatList;
        }

        private List<HircTreeItem> GetHircParents(HircTreeItem root)
        {
            var childData = new List<HircTreeItem>();
            if (root.Children != null)
            {
                foreach (var child in root.Children)
                    childData.AddRange(GetHircParents(child));
            }

            childData.Add(root);
            return childData;
        }

        private void ProcessHircObject(HircItem item, HircTreeItem parent)
        {
            if (_hircProcessChildMap.TryGetValue(item.HircType, out var func))
            {
                func(item, parent);
            }
            else
            {
                var unknownNode = new HircTreeItem() { DisplayName = $"Unknown node type {item.HircType} for Id {item.ID} in {item.OwnerFilePath}", Item = item };
                parent.Children.Add(unknownNode);
            }
        }


        protected void ProcessNext(uint hircId, HircTreeItem parent)
        {
            if (hircId == 0)
                return;

            var instances = _repository.GetHircObject(hircId);
            var hircItem = instances.FirstOrDefault();
            if (hircItem == null)
                parent.Children.Add(new HircTreeItem() { DisplayName = $"Error: Unable to find ID {hircId}" });
            else
                ProcessHircObject(hircItem, parent);
        }


        protected void ProcessNext(List<uint> ids, HircTreeItem parent)
        {
            foreach (var id in ids)
                ProcessNext(id, parent);
        }

        protected virtual string GetDisplayId(uint id, string fileName, bool hidenNameIfMissing)
        {
            var name = _repository.GetNameFromID(id, out var found);
            if (hidenNameIfMissing)
                name = "";
            if (found == true && _showId)
                name += " " + id;
            if (_showOwningBnkFile && string.IsNullOrWhiteSpace(fileName) == false)
                name += " " + fileName;
            return name;
        }

        protected string GetDisplayId(HircItem item, bool hidenNameIfMissing) => GetDisplayId(item.ID, item.OwnerFilePath, hidenNameIfMissing);

        protected Wanted GetAsType<Wanted>(HircItem instance) where Wanted : class
        {
            var wanted = instance as Wanted;
            if (wanted == null)
                throw new Exception($"HircItem with ID {instance.ID} is of type {instance.GetType().Name} and cannot be converted to {typeof(Wanted).Name}.");
            return wanted;
        }
    }
}
