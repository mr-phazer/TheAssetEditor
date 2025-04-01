﻿using System.Linq;
using Editors.Audio.AudioEditor.DataGrids;
using Editors.Audio.Storage;

namespace Editors.Audio.AudioEditor.AudioProjectData
{
    public class StateGroupDataService : IAudioProjectDataService
    {
        private readonly IAudioEditorService _audioEditorService;
        private readonly IAudioRepository _audioRepository;

        public StateGroupDataService(IAudioEditorService audioEditorService, IAudioRepository audioRepository)
        {
            _audioEditorService = audioEditorService;
            _audioRepository = audioRepository;
        }

        public void AddToAudioProject()
        {
            var audioProjectEditorRow = DataGridHelpers.GetAudioProjectEditorDataGridRow(_audioEditorService);
            var state = AudioProjectHelpers.CreateStateFromDataGridRow(audioProjectEditorRow);
            var stateGroup = AudioProjectHelpers.GetStateGroupFromName(_audioEditorService, _audioEditorService.GetSelectedExplorerNode().Name);
            AudioProjectHelpers.InsertStateAlphabetically(stateGroup, state);
        }

        public void RemoveFromAudioProject()
        {
            var stateGroup = AudioProjectHelpers.GetStateGroupFromName(_audioEditorService, _audioEditorService.GetSelectedExplorerNode().Name);
            var dataGridRowsCopy = _audioEditorService.GetSelectedViewerRows().ToList(); // Create a copy to prevent an error where dataGridRows is modified while being iterated over
            foreach (var dataGridRow in dataGridRowsCopy)
            {
                var state = AudioProjectHelpers.GetStateFromDataGridRow(dataGridRow, stateGroup);
                stateGroup.States.Remove(state);
                _audioEditorService.GetViewerDataGrid().Remove(dataGridRow);
            }
        }
    }
}
