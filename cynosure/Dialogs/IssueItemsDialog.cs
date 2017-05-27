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
    public class IssueItemsDialog : BaseDialog
    {
        public override Task StartAsync(IDialogContext context)
        {
            if (!context.UserData.TryGetValue(@"profile", out _standup))
            {
                _standup = new Standup();
            }
            EnterIssues(context);
            return Task.CompletedTask;
        }
        
        private void EnterIssues(IDialogContext context)
        {
            var text = Standup.ItemsSummary("Items already recorded as blocking:", _standup.Issues);
            string promptText;
            if (_standup.Issues.Any())
            {
                promptText = "What other blockers you are facing right now?";
            }
            else
            {
                promptText = "What blockers are you facing at the moment?";
            }
            var promptOptions = new PromptOptions<string>(
                text + "\n\n\n\n" + promptText,
                speak: promptText
                );

            var prompt = new PromptDialog.PromptString(promptOptions);
            context.Call<string>(prompt, IssuesItemEnteredAsync);
        }

        private async Task IssuesItemEnteredAsync(IDialogContext context, IAwaitable<string> result)
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
                context.Call<string>(prompt, IssuesItemEnteredAsync);
            }
            else if (IsLastInput(input))
            {
                await SummaryReportAsync(context);
                context.Done(_standup);
            }
            else
            {
                _standup.Issues.Add(input);
                context.UserData.SetValue(@"profile", _standup);
                EnterIssues(context);
            }
        }

        internal override List<Command> Commands()
        {
            List<Command> commands = new List<Command>();
            commands.Add(new Command("Finished", "Finish editing issues"));
            return commands;
        }
    }
}