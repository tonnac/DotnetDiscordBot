﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DiscordBot.Resource {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Localization {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Localization() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DiscordBot.Resource.Localization", typeof(Localization).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Added to queue.
        /// </summary>
        internal static string addedQueue {
            get {
                return ResourceManager.GetString("addedQueue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to When attacking, gain gold equal to the damage dealt.
        ///   When defeated, a certain amount of gold is paid to the last attacker.
        ///   In case of defeat, gold is paid to the person with the highest deal amount equal to the deal amount..
        /// </summary>
        internal static string bossattack_Description {
            get {
                return ResourceManager.GetString("bossattack_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Show boss commands and descriptions.
        /// </summary>
        internal static string bosshelp_Description {
            get {
                return ResourceManager.GetString("bosshelp_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Shows current boss information..
        /// </summary>
        internal static string bossinfo_Description {
            get {
                return ResourceManager.GetString("bossinfo_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Shows the boss list..
        /// </summary>
        internal static string bosslist_Description {
            get {
                return ResourceManager.GetString("bosslist_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Check Your DMs!.
        /// </summary>
        internal static string CheckDm {
            get {
                return ResourceManager.GetString("CheckDm", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} rolls {1} ({2}).
        /// </summary>
        internal static string Dice {
            get {
                return ResourceManager.GetString("Dice", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Roll the dice..
        /// </summary>
        internal static string dice_Description {
            get {
                return ResourceManager.GetString("dice_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DiceValue.
        /// </summary>
        internal static string DiceValue {
            get {
                return ResourceManager.GetString("DiceValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Disconnected.
        /// </summary>
        internal static string disconnected {
            get {
                return ResourceManager.GetString("disconnected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sends calls to registered members..
        /// </summary>
        internal static string doaram_Description {
            get {
                return ResourceManager.GetString("doaram_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Deleted from the member..
        /// </summary>
        internal static string doaramdelete_Description {
            get {
                return ResourceManager.GetString("doaramdelete_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Register as a member..
        /// </summary>
        internal static string doaramregister_Description {
            get {
                return ResourceManager.GetString("doaramregister_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do Dice Gambling. You can enter the amount (ex. ddg 1000).
        /// </summary>
        internal static string dodicegamble_Description {
            get {
                return ResourceManager.GetString("dodicegamble_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Gamble Funds. If you win, you take all the money collected..
        /// </summary>
        internal static string dofundsgamble_Description {
            get {
                return ResourceManager.GetString("dofundsgamble_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Donate to the donation box. You can enter the amount (ex. dn 1000).
        /// </summary>
        internal static string donation_Description {
            get {
                return ResourceManager.GetString("donation_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Duration.
        /// </summary>
        internal static string Duration {
            get {
                return ResourceManager.GetString("Duration", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There is no queue.
        /// </summary>
        internal static string ErrorNotQueue {
            get {
                return ResourceManager.GetString("ErrorNotQueue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Leave voice channel you are currently participating in..
        /// </summary>
        internal static string exit_Description {
            get {
                return ResourceManager.GetString("exit_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do Fishing..
        /// </summary>
        internal static string fishing_Description {
            get {
                return ResourceManager.GetString("fishing_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Show fishing commands and descriptions.
        /// </summary>
        internal static string fishinghelp_Description {
            get {
                return ResourceManager.GetString("fishinghelp_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Shows the fish list..
        /// </summary>
        internal static string fishlist_Description {
            get {
                return ResourceManager.GetString("fishlist_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Shows the gamble list..
        /// </summary>
        internal static string gamblegamelist_Description {
            get {
                return ResourceManager.GetString("gamblegamelist_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Show gamble commands and descriptions.
        /// </summary>
        internal static string gamblehelp_Description {
            get {
                return ResourceManager.GetString("gamblehelp_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Shows the Game Ranking board..
        /// </summary>
        internal static string gameranking_Description {
            get {
                return ResourceManager.GetString("gameranking_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Send a message to chatgpt..
        /// </summary>
        internal static string gpt_Description {
            get {
                return ResourceManager.GetString("gpt_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Saves the current song to your Direct Messages.
        /// </summary>
        internal static string grab_Description {
            get {
                return ResourceManager.GetString("grab_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Show all commands and descriptions.
        /// </summary>
        internal static string help_Description {
            get {
                return ResourceManager.GetString("help_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Stop the music and leave the voice channel.
        /// </summary>
        internal static string leave_Description {
            get {
                return ResourceManager.GetString("leave_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Play music.
        ///This has a lower priority than music added with play..
        /// </summary>
        internal static string longplay_Description {
            get {
                return ResourceManager.GetString("longplay_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Shows my game information..
        /// </summary>
        internal static string myinfo_Description {
            get {
                return ResourceManager.GetString("myinfo_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You need to join a voice channel first!.
        /// </summary>
        internal static string NotInChannel {
            get {
                return ResourceManager.GetString("NotInChannel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You must be in the same channel as {0}.
        /// </summary>
        internal static string NotInSameChannel {
            get {
                return ResourceManager.GetString("NotInSameChannel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to NowPlaing.
        /// </summary>
        internal static string NowPlaying {
            get {
                return ResourceManager.GetString("NowPlaying", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to See what song is currently playing.
        /// </summary>
        internal static string nowplaying_Description {
            get {
                return ResourceManager.GetString("nowplaying_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Pause the currently playing music.
        /// </summary>
        internal static string pause_Description {
            get {
                return ResourceManager.GetString("pause_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You don&apos;t have permission..
        /// </summary>
        internal static string Permission {
            get {
                return ResourceManager.GetString("Permission", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Plays audio from YouTube.
        /// </summary>
        internal static string play_Description {
            get {
                return ResourceManager.GetString("play_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Usage: {0}play YouTube URL or Video Name.
        /// </summary>
        internal static string play_Usage {
            get {
                return ResourceManager.GetString("play_Usage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Position in queue.
        /// </summary>
        internal static string positionInQueue {
            get {
                return ResourceManager.GetString("positionInQueue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Queue.
        /// </summary>
        internal static string Queue {
            get {
                return ResourceManager.GetString("Queue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Show the music queue and now playing..
        /// </summary>
        internal static string queue_Description {
            get {
                return ResourceManager.GetString("queue_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remove song from the queue.
        /// </summary>
        internal static string remove_Description {
            get {
                return ResourceManager.GetString("remove_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Usage: {0}remove &lt;Queue Number&gt;.
        /// </summary>
        internal static string remove_Usage {
            get {
                return ResourceManager.GetString("remove_Usage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Requested By.
        /// </summary>
        internal static string RequestedBy {
            get {
                return ResourceManager.GetString("RequestedBy", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Resume currently playing music.
        /// </summary>
        internal static string resume_Description {
            get {
                return ResourceManager.GetString("resume_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Roller.
        /// </summary>
        internal static string Roller {
            get {
                return ResourceManager.GetString("Roller", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SaveMusic.
        /// </summary>
        internal static string SaveMusic {
            get {
                return ResourceManager.GetString("SaveMusic", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SaveTime.
        /// </summary>
        internal static string SaveTime {
            get {
                return ResourceManager.GetString("SaveTime", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Seek to a position in the song.
        /// </summary>
        internal static string seek_Description {
            get {
                return ResourceManager.GetString("seek_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Usage: {0}seek &lt;number h/m/s&gt; Example - {0}seek 2m 10s.
        /// </summary>
        internal static string seek_Usage {
            get {
                return ResourceManager.GetString("seek_Usage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Skip the currently playing song.
        /// </summary>
        internal static string skip_Description {
            get {
                return ResourceManager.GetString("skip_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Take your donation from the donation box..
        /// </summary>
        internal static string thanks_Description {
            get {
                return ResourceManager.GetString("thanks_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Total Length.
        /// </summary>
        internal static string TotalLength {
            get {
                return ResourceManager.GetString("TotalLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Total Songs.
        /// </summary>
        internal static string TotalSongs {
            get {
                return ResourceManager.GetString("TotalSongs", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Strengthen the ring. (attack hit chance, dice gamble + reinforcement value).
        /// </summary>
        internal static string upgradering_Description {
            get {
                return ResourceManager.GetString("upgradering_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Shows a list of upgrade equip probabilities. Step input possible (ex. ul 0).
        /// </summary>
        internal static string upgradesuccesspercentagelist_Description {
            get {
                return ResourceManager.GetString("upgradesuccesspercentagelist_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Strengthen your weapon. (Damage + Enhanced Value).
        /// </summary>
        internal static string upgradeweapon_Description {
            get {
                return ResourceManager.GetString("upgradeweapon_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Up Next.
        /// </summary>
        internal static string UpNext {
            get {
                return ResourceManager.GetString("UpNext", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Roll the WOW type dice..
        /// </summary>
        internal static string wowdice_Description {
            get {
                return ResourceManager.GetString("wowdice_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Wrong Parameter.
        /// </summary>
        internal static string wrongDice {
            get {
                return ResourceManager.GetString("wrongDice", resourceCulture);
            }
        }
    }
}
