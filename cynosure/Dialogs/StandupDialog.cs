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
            EnterDone(context);
            return Task.CompletedTask;
        }

        Standup _standup;
        private void EnterDone(IDialogContext context)
        {
            if (!context.UserData.TryGetValue(@"profile", out _standup))
            {
                _standup = new Standup();
            }

            var text = "What did you complete in the last cycle?";
            var promptOptions = new PromptOptions<string>(
                text,
                speak: text
                );

            var prompt = new PromptDialog.PromptString(promptOptions);
            context.Call<string>(prompt, DoneItemEnteredAsync);
        }

        private async Task DoneItemEnteredAsync(IDialogContext context, IAwaitable<string> result)
        {
            string input = await result;
            _standup.Done.Add(input);

            var text = "Did you complete anything else in the last cycle?";
            PromptDialog.Confirm(context, FinishedDoneAsync, text);
            /*
            var promptOptions = new PromptOptions(
                prompt: text,
                speak: text
                );
            var prompt = new PromptDialog.PromptConfirm(promptOptions);
            context.Call(prompt, FinishedDoneAsync);
            */
        }

        private async Task FinishedDoneAsync(IDialogContext context, IAwaitable<bool> result)
        {
            bool done = await result;
            if (done)
            {
                EnterDone(context);
            } else
            {
                await context.PostAsync("Great. Thanks.");
                EnterCommitted(context);
            }
        }

        private void EnterCommitted(IDialogContext context)
        {
            var text = "What are you focussing on now?";
            var promptOptions = new PromptOptions<string>(
                text,
                speak: text
                );

            var prompt = new PromptDialog.PromptString(promptOptions);
            context.Call<string>(prompt, CommittedItemEnteredAsync);
        }

        private async Task CommittedItemEnteredAsync(IDialogContext context, IAwaitable<string> result)
        {
            string input = await result;
            _standup.Committed.Add(input);
            var text = "Do you have any other focus items right now?";
            PromptDialog.Confirm(context, FinishedCommittedAsync, text);
        }

        private async Task FinishedCommittedAsync(IDialogContext context, IAwaitable<bool> result)
        {
            bool done = await result;
            if (done)
            {
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
            var text = "Are there any issues blocking you right now?";
            var promptOptions = new PromptOptions<string>(
                text,
                speak: text
                );

            var prompt = new PromptDialog.PromptString(promptOptions);
            context.Call<string>(prompt, IssuesItemEnteredAsync);
        }

        private async Task IssuesItemEnteredAsync(IDialogContext context, IAwaitable<string> result)
        {
            string input = await result;
            _standup.Issues.Add(input);
            var text = "Any other blockers right now?";
            PromptDialog.Confirm(context, FinishedIssuesAsync, text);
        }

        private async Task FinishedIssuesAsync(IDialogContext context, IAwaitable<bool> result)
        {
            bool done = await result;
            if (done)
            {
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
            var done = await result;
            if (done)
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