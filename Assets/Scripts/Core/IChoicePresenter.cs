using System;
using System.Collections.Generic;

namespace ImmersiveSubtitles.Core
{
    public interface IChoicePresenter
    {
        void Show(IReadOnlyList<ChoiceOption> options, Action<int> onChosen); 
        void Hide();
    }
}
