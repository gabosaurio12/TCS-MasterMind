using MasterMind_Client.Helpers;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Pages.MatchPages.MatchTries.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MasterMind_Client.ViewModels.Pages.MatchPages.MatchTries.ViewModels
{
    public class MatchRowViewModel : ViewModelBase
    {
        public MatchRowViewModel(Action<MatchPinSlotViewModel> placePin)
        {
            for (int index = 0; index < MatchRules.CodeLength; index++)
            {
                PinSlots.Add(new MatchPinSlotViewModel(placePin));
                ClueSlots.Add(new MatchClueSlotViewModel());
            }
        }

        public ObservableCollection<MatchPinSlotViewModel> PinSlots { get; } =
            new ObservableCollection<MatchPinSlotViewModel>();

        public ObservableCollection<MatchClueSlotViewModel> ClueSlots { get; } =
            new ObservableCollection<MatchClueSlotViewModel>();

        public bool IsComplete => PinSlots.All(slot => slot.Colour.HasValue);

        public IReadOnlyList<PinColour> GetCode()
        {
            return PinSlots.Select(slot => slot.Colour.Value).ToList();
        }

        public void ApplyClues(IList<string> clues)
        {
            for (int index = 0; index < ClueSlots.Count && index < clues.Count; index++)
            {
                ClueSlots[index].Kind = MatchColourMapper.ToClueKind(clues[index]);
            }
        }

        public void CopyFrom(MatchRowViewModel source)
        {
            for (int index = 0; index < PinSlots.Count; index++)
            {
                PinSlots[index].Colour = source.PinSlots[index].Colour;
                ClueSlots[index].Kind = source.ClueSlots[index].Kind;
            }
        }

        public void Clear()
        {
            foreach (var slot in PinSlots)
            {
                slot.Colour = null;
            }

            foreach (var slot in ClueSlots)
            {
                slot.Kind = ClueColour.None;
            }
        }
    }
}
