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

        Standup _standup;

        [MethodBind]
        [ScorableGroup(0)]
        private async Task ActivityHandler(IDialogContext context, IActivity activity)
        { 
            try
            {
                switch (activity.Type)
                {
                    case ActivityTypes.Message:
                        this.ContinueWithNextGroup();
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
            catch (Microsoft.Bot.Builder.Internals.Fibers.InvalidNeedException ex)
            {
                var telemetry = new TelemetryClient();
                telemetry.TrackException(ex);
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                Activity reply = ((Activity)activity).CreateReply("Sorry, I'm having some difficulties here. I have to reboot myself. Let's start over");
                await connector.Conversations.ReplyToActivityAsync(reply);
                StateClient stateClient = activity.GetStateClient();
                await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
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
        public void StartStandup(IDialogContext context, IActivity activity)
        {
            var telemetry = new TelemetryClient();
            telemetry.TrackEvent("Start Standup");
            context.Call<Standup>(new StandupDialog(), standupUpdatedAsync);
        }

        [RegexPattern("standup summar|summary|standup report|report")]
        [ScorableGroup(1)]
        public async Task StandupSummary(IDialogContext context, IActivity activity)
        {
            var telemetry = new TelemetryClient();
            telemetry.TrackEvent("Summarize Standup");
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
            await this.DefaultAsync(context, activity);
        }

        [MethodBind]
        [ScorableGroup(2)]
        public async Task DefaultAsync(IDialogContext context, IActivity activity)
        {
            var telemetry = new TelemetryClient();
            try
            {
                telemetry.TrackEvent("Display Help");
                context.Call(new HelpDialog(), AfterDialog);
            }
            catch (Microsoft.Bot.Builder.Internals.Fibers.InvalidNeedException ex)
            {
                telemetry.TrackException(ex);
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                Activity reply = ((Activity)activity).CreateReply("Sorry, I'm having some difficulties here. I have to reboot myself. Let's start over.");
                await connector.Conversations.ReplyToActivityAsync(reply);
                StateClient stateClient = activity.GetStateClient();
                await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                telemetry.TrackEvent("Cleared users state.");
            }
        }

        [RegexPattern("hello|hi")]
        [ScorableGroup(1)]
        public async Task Hello(IDialogContext context, IActivity activity)
        {
            var telemetry = new TelemetryClient();
            telemetry.TrackEvent("Say Hi");
            await context.PostAsync(@"Hello, I'm Cynosure. Say 'help' to learn more about what I can do.");
            context.Done(true);
        }

        private static async Task AfterDialog(IDialogContext context, IAwaitable<object> result)
        {
            context.Done<object>(null);
        }
    }
}