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

        override protected async Task TextEnteredAsync(IDialogContext context, IAwaitable<string> result)
        {
            Standup standup = GetCurrentStandup(context);
            string input = await result;
            if (IsHelp(input))
            {
                await DisplayHelpCard(context);
                var promptOptions = new PromptOptions<string>(
                    "What do you want to do?",
                    speak: "What do you want to do?"
                    );
                var prompt = new PromptDialog.PromptString(promptOptions);
                context.Call<string>(prompt, TextEnteredAsync);
            }
            else if (IsLastInput(input))
            {
                await SummaryReportAsync(context);
                context.Done(standup);
            }
            else
            {
                standup.Committed.Add(input);
                context.UserData.SetValue(@"profile", standup);
                RequestInput(context);
            }
        }

        internal override List<Command> Commands()
        {
            List<Command> commands = new List<Command>();
            commands.Add(new Command("Finished", "Finish editing the committed items"));
            return commands;
        }
    }
}