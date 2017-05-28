using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace cynosure.Dialogs
{
    [Serializable]
    public abstract class AbstractItemDialog: AbstractBaseDialog
    {
        abstract protected void EnterItem(IDialogContext context);
        abstract protected Task ItemEnteredAsync(IDialogContext context, IAwaitable<string> result);
    }
}