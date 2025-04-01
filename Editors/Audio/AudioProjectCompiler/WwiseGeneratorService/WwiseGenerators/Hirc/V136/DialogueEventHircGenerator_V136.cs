﻿using System.Collections.Generic;
using System.Linq;
using Editors.Audio.AudioEditor.AudioProjectData;
using Shared.GameFormats.Wwise.Enums;
using Shared.GameFormats.Wwise.Hirc;
using Shared.GameFormats.Wwise.Hirc.V136;
using Shared.GameFormats.Wwise.Hirc.V136.Shared;

namespace Editors.Audio.AudioProjectCompiler.WwiseGeneratorService.WwiseGenerators.Hirc.V136
{
    public class DialogueEventHircGenerator_V136 : IWwiseHircGeneratorService
    {
        public HircItem GenerateHirc(AudioProjectItem audioProjectItem, SoundBank soundBank)
        {
            var audioProjectDialogueEvent = audioProjectItem as DialogueEvent;
            var dialogueEventHirc = CreateDialogueEvent(audioProjectDialogueEvent);
            dialogueEventHirc.UpdateSectionSize();
            return dialogueEventHirc;
        }

        private static CAkDialogueEvent_V136 CreateDialogueEvent(DialogueEvent audioProjectDialogueEvent)
        {
            var dialogueEventHirc = new CAkDialogueEvent_V136();
            dialogueEventHirc.ID = audioProjectDialogueEvent.ID;
            dialogueEventHirc.HircType = AkBkHircType.Dialogue_Event;
            dialogueEventHirc.Probability = 100;
            dialogueEventHirc.Arguments = CreateArguments(audioProjectDialogueEvent, dialogueEventHirc);
            dialogueEventHirc.TreeDepth = (uint)dialogueEventHirc.Arguments.Count;
            dialogueEventHirc.AkDecisionTree = CreateDecisionTree(audioProjectDialogueEvent);
            dialogueEventHirc.TreeDataSize = (uint)dialogueEventHirc.AkDecisionTree.Nodes.Count * new AkDecisionTree_V136.Node_V136().GetSize();
            dialogueEventHirc.Mode = (byte)AkMode.BestMatch;
            dialogueEventHirc.AkPropBundle0 = new AkPropBundle_V136() { PropsList = new List<AkPropBundle_V136.PropBundleInstance_V136>() };
            dialogueEventHirc.AkPropBundle1 = new AkPropBundleMinMax_V136() { PropsList = new List<AkPropBundleMinMax_V136.AkPropBundleInstance_V136>() };
            return dialogueEventHirc;
        }

        private static List<AkGameSync_V136> CreateArguments(DialogueEvent audioProjectDialogueEvent, CAkDialogueEvent_V136 dialogueEventHirc)
        {
            var arguments = new List<AkGameSync_V136>();
            foreach (var statePathNode in audioProjectDialogueEvent.StatePaths[0].Nodes)
            {
                var argument = new AkGameSync_V136
                {
                    GroupID = statePathNode.StateGroup.ID,
                    GroupType = AkGroupType.State
                };
                arguments.Add(argument);
            }
            return arguments;
        }

        private static AkDecisionTree_V136 CreateDecisionTree(DialogueEvent audioProjectDialogueEvent)
        {
            var rootNode = CreateNode(key: 0, audioNodeId: 0, childrenUIdx: 0, childrenUCount: 0, weight: 50, probability: 100);
            foreach (var statePath in audioProjectDialogueEvent.StatePaths)
            {
                var currentNode = rootNode;
                for (var i = 0; i < statePath.Nodes.Count; i++)
                {
                    var statePathNode = statePath.Nodes[i];
                    var stateKey = statePathNode.State.ID;

                    var existingChild = currentNode.Nodes.FirstOrDefault(n => n.Key == stateKey);
                    if (existingChild != null)
                        currentNode = existingChild;
                    else
                    {
                        uint audioNodeId;
                        if (i == statePath.Nodes.Count - 1)
                            audioNodeId = statePath.RandomSequenceContainer.ID;
                        else
                            audioNodeId = 0;

                        var newNode = CreateNode(key: stateKey, audioNodeId: audioNodeId, childrenUIdx: 0, childrenUCount: 0, weight: 50, probability: 100);
                        currentNode.Nodes.Add(newNode);
                        currentNode = newNode;
                    }
                }
            }

            var flattenedDecisionTree = FlattenDecisionTree(rootNode);
            return new AkDecisionTree_V136
            {
                DecisionTree = rootNode,
                Nodes = flattenedDecisionTree
            };
        }

        private static List<AkDecisionTree_V136.Node_V136> FlattenDecisionTree(AkDecisionTree_V136.Node_V136 rootNode)
        {
            var flattenedDecisionTree = new List<AkDecisionTree_V136.Node_V136>();
            flattenedDecisionTree.Add(rootNode);
            FlattenChildren(rootNode, flattenedDecisionTree);
            return flattenedDecisionTree;
        }

        private static void FlattenChildren(AkDecisionTree_V136.Node_V136 node, List<AkDecisionTree_V136.Node_V136> flattenedDecisionTree)
        {
            if (node.Nodes == null || node.Nodes.Count == 0)
            {
                node.ChildrenIdx = 0;
                node.ChildrenCount = 0;
                return;
            }

            node.ChildrenIdx = (ushort)flattenedDecisionTree.Count;
            node.ChildrenCount = (ushort)node.Nodes.Count;

            foreach (var child in node.Nodes)
                flattenedDecisionTree.Add(child);

            foreach (var child in node.Nodes)
                FlattenChildren(child, flattenedDecisionTree);
        }

        public static AkDecisionTree_V136.Node_V136 CreateNode(uint key, uint audioNodeId, ushort childrenUIdx, ushort childrenUCount, ushort weight, ushort probability)
        {
            return new AkDecisionTree_V136.Node_V136
            {
                Key = key,
                AudioNodeID = audioNodeId,
                ChildrenIdx = childrenUIdx,
                ChildrenCount = childrenUCount,
                Weight = weight,
                Probability = probability,
                Nodes = new List<AkDecisionTree_V136.Node_V136>()
            };
        }
    }
}
