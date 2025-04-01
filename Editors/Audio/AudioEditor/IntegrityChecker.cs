﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Editors.Audio.Storage;
using static Editors.Audio.GameSettings.Warhammer3.DialogueEvents;
using static Editors.Audio.GameSettings.Warhammer3.SoundBanks;

namespace Editors.Audio.AudioEditor
{
    // TODO: Probably need something to check all audio files in the project are where they say they are
    public class IntegrityChecker
    {
        private readonly IAudioRepository _audioRepository;

        public IntegrityChecker(IAudioRepository audioRepository)
        {
            _audioRepository = audioRepository;
        }

        public void CheckDialogueEventIntegrity(List<(string Name, Wh3SoundBankSubtype SoundBank, DialogueEventPreset[] DialogueEventPreset, bool Recommended)> dialogueEventData)
        {
            var gameDialogueEvents = _audioRepository.StateGroupsLookupByDialogueEvent.Keys.ToList();
            var audioEditorDialogueEvents = dialogueEventData.Select(data => data.Name).ToList();

            var areGameAndAudioEditorDialogueEventsMatching = new HashSet<string>(gameDialogueEvents).SetEquals(audioEditorDialogueEvents);

            if (!areGameAndAudioEditorDialogueEventsMatching)
            {
                var dialogueEventsOnlyInGame = gameDialogueEvents.Except(audioEditorDialogueEvents).ToList();
                var dialogueEventsOnlyInAudioEditor = audioEditorDialogueEvents.Except(gameDialogueEvents).ToList();

                var dialogueEventsOnlyInGameText = string.Join("\n - ", dialogueEventsOnlyInGame);
                var dialogueEventsOnlyInAudioEditorText = string.Join("\n - ", dialogueEventsOnlyInAudioEditor);

                var message = $"Dialogue Event integrity check failed." +
                    $"\n\nThis is due to a change in the game's Dialogue Events by CA." +
                    $"\n\nPlease report this error to the AssetEditor development team.";
                var dialogueEventsOnlyInGameMessage = $"\n\nGame Dialogue Events not in the Audio Editor:\n - {dialogueEventsOnlyInGameText}";
                var dialogueEventsOnlyInAudioEditorMessage = $"\n\nAudio Editor Dialogue Events not in the game:\n - {dialogueEventsOnlyInAudioEditorText}";

                if (dialogueEventsOnlyInGame.Count > 0)
                    message += dialogueEventsOnlyInGameMessage;

                if (dialogueEventsOnlyInAudioEditor.Count > 0)
                    message += dialogueEventsOnlyInAudioEditorMessage;

                MessageBox.Show(message, "Error");
            }
        }

        public void CheckAudioProjectDialogueEventIntegrity(IAudioEditorService audioEditorService)
        {
            var audioProjectDialogueEventsWithStateGroups = new Dictionary<string, List<string>>();

            foreach (var soundBank in audioEditorService.AudioProject.SoundBanks)
            {
                if (soundBank.SoundBankType == Wh3SoundBankType.DialogueEventSoundBank)
                    foreach (var dialogueEvent in soundBank.DialogueEvents)
                    {
                        var firstStatePath = dialogueEvent.StatePaths.FirstOrDefault();
                        if (firstStatePath != null)
                        {
                            var stateGroups = firstStatePath.Nodes.Select(node => node.StateGroup.Name).ToList();
                            audioProjectDialogueEventsWithStateGroups[dialogueEvent.Name] = stateGroups;
                        }
                    }
            }

            foreach (var dialogueEvent in audioProjectDialogueEventsWithStateGroups)
            {
                var gameDialogueEventStateGroups = _audioRepository.StateGroupsLookupByDialogueEvent[dialogueEvent.Key];
                var audioProjectDialogueEventStateGroups = dialogueEvent.Value;
                if (!gameDialogueEventStateGroups.SequenceEqual(audioProjectDialogueEventStateGroups))
                    audioEditorService.DialogueEventsWithStateGroupsWithIntegrityError[dialogueEvent.Key] = audioProjectDialogueEventStateGroups;
            }

            if (audioEditorService.DialogueEventsWithStateGroupsWithIntegrityError.Count == 0)
                return;

            var message = $"Dialogue Events State Groups integrity check failed." +
                $"\n\nThis is likely due to a change in the State Groups by a recent update to the game or you've done something silly with the file." +
                $"\n\nWhen browsing the affected Dialogue Events you will see the rows have been updated to accommodate for the new State Groups, and if any of the old State Groups are no longer used they will have been removed." +
                $" The new State Group(s) will have no State set in them so you need to click Update Row and add the State(s). " +
                $"\n\nAffected Dialogue Events:";

            foreach (var dialogueEvent in audioEditorService.DialogueEventsWithStateGroupsWithIntegrityError)
            {
                message += $"\n\nDialogue Event: {dialogueEvent.Key}";

                var audioProjectDialogueEventStateGroups = dialogueEvent.Value;
                var audioProjectDialogueEventStateGroupsText = string.Join("-->", audioProjectDialogueEventStateGroups);
                message += $"\n\nOld State Groups: {audioProjectDialogueEventStateGroupsText}";

                var gameDialogueEventStateGroups = _audioRepository.StateGroupsLookupByDialogueEvent[dialogueEvent.Key];
                var gameDialogueEventStateGroupsText = string.Join("-->", gameDialogueEventStateGroups);
                message += $"\n\nNew State Groups: {gameDialogueEventStateGroupsText}";
            }

            MessageBox.Show(message, "Error");
        }
    }
}
