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
    public class StandupDialog : IDialog<Standup>
    {
        public Task StartAsync(IDialogContext context)
        {
            if (!context.UserData.TryGetValue(@"profile", out _standup))
            {
                _standup = new Standup();
            }
            EnterDone(context);
            return Task.CompletedTask;
        }

        Standup _standup;
        private void EnterDone(IDialogContext context)
        {
            var text = Standup.ItemsSummary("Items already recorded as done:", _standup.Done);
            string promptText;
            if (_standup.Done.Any())
            {
                promptText = "What else did you complete in the last cycle?";
            } else
            {
                promptText = "What did you complete in the last cycle?";
            }
            var promptOptions = new PromptOptions<string>(
                text + "\n\n\n\n" + promptText,
                speak: promptText
                );

            var prompt = new PromptDialog.PromptString(promptOptions);
            context.Call<string>(prompt, DoneItemEnteredAsync);
        }

        private async Task DoneItemEnteredAsync(IDialogContext context, IAwaitable<string> result)
        {
            string input = await result;
            if (input.ToLower() != "nothing" && input.ToLower() != "no")
            {
                _standup.Done.Add(input);
                EnterDone(context);
            }
            else
            {
                await context.PostAsync("Great. Thanks.");
                EnterCommitted(context);
            }
        }

        private void EnterCommitted(IDialogContext context)
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
            context.Call<string>(prompt, CommittedItemEnteredAsync);
        }

        private async Task CommittedItemEnteredAsync(IDialogContext context, IAwaitable<string> result)
        {
            string input = await result;
            if (input.ToLower() != "nothing" && input.ToLower() != "no")
            {
                _standup.Committed.Add(input);
                EnterCommitted(context);
            }
            else
            {
                await context.PostAsync("Great. Thanks.");
                EnterIssues(context);
            }
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
            if (input.ToLower() != "nothing" && input.ToLower() != "no")
            {
                _standup.Issues.Add(input);
                EnterIssues(context);
            }
            else
            {
                await context.PostAsync("Great. Thanks.");
                SummaryReportAsync(context);
            }
        }

        private void SummaryReportAsync(IDialogContext context)
        {
            string summary = _standup.Summary();
            summary += "\n\n\n\nDo you want to post this standup summary?";
            PromptDialog.Confirm(context, StandupCompleteAsync, summary);
        }

        private async Task StandupCompleteAsync(IDialogContext context, IAwaitable<bool> result)
        {
            var more = await result;
            if (more)
            {
                await context.PostAsync("Great, thanks for completing your standup report.");
                context.Done(_standup);
            }
            else
            {
                EnterDone(context);
            }
        }
    }
}