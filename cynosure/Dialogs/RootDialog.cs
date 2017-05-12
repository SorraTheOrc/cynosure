using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Text.RegularExpressions;
using cynosure.Model;

namespace cynosure.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            UserProfile _profile;
            if (context.UserData.TryGetValue(@"profile", out _profile))
            {
                _profile = new UserProfile();
            }
            else
            {
                context.Call<UserProfile>(new UpdateUserProfileDialog(), profileUpdated);
            }

            var activity = await result as Activity;

            if (sayHello(activity))
            {
                await context.PostAsync($@"Hello, {_profile.FamiliarName}, I'm Cynosure. Say 'help' to learn more about what I can do.");
                return;
            }

            if (askForHelp(activity))
            {
                String help = "My main resonsibility is to run your standup for you.\n\n";
                help += "My commands:\n\n\n\n";
                help += "'start standup'   : Starts the standup conversation.\n\n";
                help += "'standup summary' : Displays a summary of the current standup reports";
                await context.PostAsync(help);
                return;
            }

            if (startStandup(activity))
            {
                context.Call<Standup>(new StandupDialog(), standupUpdatedAsync);
                return;
            }

            Standup _standup;
            if (standupSummary(activity))
            {
                if (context.UserData.TryGetValue(@"standup", out _standup))
                {
                    await context.PostAsync(_standup.Summary());
                    return;
                }
                else
                {
                    await context.PostAsync("There is no standup data right now. You can 'start standup' if you like");
                    return;
                }
            }
            
            // return our reply to the user
            await context.PostAsync($"Sorry I don't know what to do when you say '{activity.Text}'");
            context.Wait(MessageReceivedAsync);
        }

        private async Task standupUpdatedAsync(IDialogContext context, IAwaitable<Standup> result)
        {
            var standup = await result;
            context.UserData.SetValue(@"standup", standup);
            context.Wait(MessageReceivedAsync);
        }

        private async Task profileUpdated(IDialogContext context, IAwaitable<UserProfile> result)
        {
            var profile = await result;
            context.UserData.SetValue(@"profile", profile);
            context.Wait(MessageReceivedAsync);
        }

        private bool startStandup(Activity activity)
        {
            var regex = new Regex("^start standup");
            return regex.Match(activity.Text).Success;
        }

        private bool standupSummary(Activity activity)
        {
            var regex = new Regex("^standup summary");
            return regex.Match(activity.Text).Success;
        }

        private bool askForHelp(Activity activity)
        {
            var regex = new Regex("^help");
            return regex.Match(activity.Text).Success;
        }

        private bool sayHello(Activity activity)
        {
            var regex = new Regex("^hello");
            return regex.Match(activity.Text).Success;
        }
    }
}