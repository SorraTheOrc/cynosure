using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

namespace cynosure.Dialogs
{
    [Serializable]
    public class UpdateUserProfileDialog : IDialog<UserProfile>
    {
        public Task StartAsync(IDialogContext context)
        {
            EnsureProfileName(context);
            return Task.CompletedTask;
        }

        UserProfile _profile;
        private void EnsureProfileName(IDialogContext context)
        {
            if (!context.UserData.TryGetValue(@"profile", out _profile)) {
                _profile = new UserProfile();
            }

            if (string.IsNullOrWhiteSpace(_profile.FamiliarName))
            {
                PromptDialog.Text(context, NameEnteredAsync, @"What shall I call you?");
            } else
            {
                EnsureStandupTime(context);
            }
        }

        private async Task NameEnteredAsync(IDialogContext context, IAwaitable<string> result)
        {
            _profile.FamiliarName = await result;
            EnsureStandupTime(context);
        }

        private void EnsureStandupTime(IDialogContext context)
        {
            if (string.IsNullOrWhiteSpace(_profile.StandupTime))
            {
                PromptDialog.Text(context, StandupTimeEnteredAsync, @"What time do you want to run your standup?");
            } else
            {
                context.Done(_profile);
            }
        }

        private async Task StandupTimeEnteredAsync(IDialogContext context, IAwaitable<string> result)
        {
            _profile.StandupTime = await result;
            context.Done(_profile);
        }
    }
}