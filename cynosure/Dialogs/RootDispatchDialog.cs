using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Text.RegularExpressions;
using cynosure.Model;
using Microsoft.Bot.Builder.Scorables;
using Microsoft.ApplicationInsights;

namespace cynosure.Dialogs
{
    [Serializable]
    public class RootDispatchDialog : DispatchDialog
    {

        UserProfile _profile;
        Standup _standup;
        
        [MethodBind]
        [ScorableGroup(0)]
        private async Task ActivityHandler(IDialogContext context, IActivity activity)
        {
            var telemetry = new TelemetryClient();
            if (context.UserData.TryGetValue(@"profile", out _profile))
            {
                telemetry.TrackEvent("New User Profile");
                _profile = new UserProfile();
            }
            else
            {
                context.Call<UserProfile>(new UpdateUserProfileDialog(), profileUpdated);
            }

            switch (activity.Type)
            {
                case ActivityTypes.Message:
                    try
                    {
                        this.ContinueWithNextGroup();
                    } catch (Microsoft.Bot.Builder.Internals.Fibers.InvalidNeedException ex)
                    {
                        telemetry.TrackException(ex);
                        ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                        Activity reply = ((Activity)activity).CreateReply("Sorry, I'm having some difficulties here. I have to reboot myself. Let's start over");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                        StateClient stateClient = activity.GetStateClient();
                        await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                    } catch (Exception ex)
                    {
                        telemetry.TrackException(ex);
                    }
                    break;
                case ActivityTypes.ConversationUpdate:
                case ActivityTypes.ContactRelationUpdate:
                case ActivityTypes.Typing:
                case ActivityTypes.DeleteUserData:
                case ActivityTypes.Ping:
                default:
                    break;
            }
        }

        private async Task standupUpdatedAsync(IDialogContext context, IAwaitable<Standup> result)
        {
            var standup = await result;
            context.UserData.SetValue(@"standup", standup);
        }

        private async Task profileUpdated(IDialogContext context, IAwaitable<UserProfile> result)
        {
            var profile = await result;
            context.UserData.SetValue(@"profile", profile);
        }

        [RegexPattern("start standup|standup|start")]
        [ScorableGroup(1)]
        public async Task StartStandup(IDialogContext context, IActivity activity)
        {
            context.Call<Standup>(new StandupDialog(), standupUpdatedAsync);
            return;
        }

        [RegexPattern("standup summar|summary|standup report|report")]
        [ScorableGroup(1)]
        public async Task StandupSummary(IDialogContext context, IActivity activity)
        {
            if (context.UserData.TryGetValue(@"standup", out _standup))
            {
                await context.PostAsync(_standup.Summary());
            }
            else
            {
                await context.PostAsync("There is no standup data right now. You can 'start standup' if you like");
            }
            context.Done(true);
;        }

        [RegexPattern("help")]
        [ScorableGroup(1)]
        public async Task Help(IDialogContext context, IActivity activity)
        {
            this.Default(context, activity);
        }

        [MethodBind]
        [ScorableGroup(2)]
        public void Default(IDialogContext context, IActivity activity)
        {
            context.Call(new HelpDialog(), AfterDialog);
        }

        [RegexPattern("hello|hi")]
        [ScorableGroup(1)]
        public async Task Hello(IDialogContext context, IActivity activity)
        {
            await context.PostAsync($@"Hello, {_profile.FamiliarName}, I'm Cynosure. Say 'help' to learn more about what I can do.");
            context.Done(true);
        }

        private static async Task AfterDialog(IDialogContext context, IAwaitable<object> result)
        {
            context.Done<object>(null);
        }
    }
}