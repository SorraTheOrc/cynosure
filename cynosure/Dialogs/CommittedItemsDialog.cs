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
        public override Task StartAsync(IDialogContext context)
        {
            if (!context.UserData.TryGetValue(@"profile", out _standup))
            {
                _standup = new Standup();
            }
            EnterItem(context);
            return Task.CompletedTask;
        }

        override protected void EnterItem(IDialogContext context)
        {
            var text = Standup.ItemsSummary("Items already recorded as focus items for today:", _standup.Committed);
            string promptText;
            if (_standup.Committed.Any())
            {
                promptText = "What else are you focusing on today?";
            }
            else
            {
                promptText = "What will you focus on today?";
            }
            var promptOptions = new PromptOptions<string>(
                text + "\n\n\n\n" + promptText,
                speak: promptText
                );

            var prompt = new PromptDialog.PromptString(promptOptions);
            context.Call<string>(prompt, ItemEnteredAsync);
        }

        override protected async Task ItemEnteredAsync(IDialogContext context, IAwaitable<string> result)
        {
            string input = await result;
            if (IsHelp(input))
            {
                await DisplayHelpCard(context);
                var promptOptions = new PromptOptions<string>(
                    "What do you want to do?",
                    speak: "What do you want to do?"
                    );
                var prompt = new PromptDialog.PromptString(promptOptions);
                context.Call<string>(prompt, ItemEnteredAsync);
            }
            else if (IsLastInput(input))
            {
                await SummaryReportAsync(context);
                context.Done(_standup);
            }
            else
            {
                _standup.Committed.Add(input);
                context.UserData.SetValue(@"profile", _standup);
                EnterItem(context);
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