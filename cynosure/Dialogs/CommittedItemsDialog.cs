using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using cynosure.Model;

namespace cynosure.Dialogs
{
    [Serializable]
    public class CommittedItemsDialog : AbstractItemDialog
    {
        protected override string GetHeader(IDialogContext context)
        {
            return Standup.ItemsSummary("Items already recorded as focus items for today:", GetCurrentStandup(context).Committed);
        }

        protected override List<string> GetItems(IDialogContext context)
        {
            return GetCurrentStandup(context).Committed;
        }

        protected override string GetPromptText(IDialogContext context)
        {
            string promptText;
            if (GetItems(context).Any())
            {
                promptText = "What else are you focusing on today?";
            }
            else
            {
                promptText = "What will you focus on today?";
            }
            return promptText;
        }

        override protected async Task ProcessDialogInput(IDialogContext context, string input)
        {
            Standup standup = GetCurrentStandup(context);
            standup.Committed.Add(input);
            context.UserData.SetValue(@"profile", standup);
            RequestInput(context);
        }

        internal override List<Command> Commands()
        {
            List<Command> commands = new List<Command>();
            commands.Add(new Command("Finished", "Finish editing the committed items"));
            commands.Add(new Command("Add <item> to committed items.", "Add an item to the committed items list.", Command.CommandType.Sentence));
            return commands;
        }

        internal override string GetCurrentDialogType()
        {
            return "committed";
        }
    }
}