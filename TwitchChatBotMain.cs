//A big thankyou goes out to HardlyDifficult
//Go watch all of his youtube videos @https://www.youtube.com/channel/UC3bHnBF2Q-u-1NEYG0Xwgeg
//Signing off, RobbieW.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq; //This is commented out to keep Intellisense clean
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;

namespace TwitchChatBot
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Okay, list of shit to do:
        /// 1.Parse the vote shit properly and act accordingly
        /// 2.Put the pwd in a json or something
        /// 3.Make a scroll wheel for client
        /// 4.Send a message every 10 minutes
        /// 5.Remember timeout 'default=600, max=1209600'
        /// 6.GET THE USERS DAMN IT WE NEED THAT
        /// 7.See if there is a way to log a chat side ban
        /// 8.Find all of the TwitchCmds ex:swriter.WriteLine($"JOIN #{cName}"); "JOIN"
        /// 9.Periodically host different streamers, every 30 minutes.
        /// 10.Add ALL commands to bot side. ALLLLL.
        /// 11.Add other stuff for cmds i.e. Hardware cmds
        /// Git Gud.
        /// </summary>

        TcpClient tcpClient; //New Transmission Control Protocol instance.
        StreamReader sreader; //New Stream Reader
        StreamWriter swriter; //New stream Writer

        //uname is short for Username, I'm lazy.
        string uname = "robbiew_yt", cName = "robbiew_yt",
            pwd = "oauth:rpxjvmttul6gjuqzwbncvy3l1soltj", chatInfo; //Client Info

        string senderr; //Gets set to senders user name

        string[] cmds = {"Use !help or !cmds or !commands to get a list of all the commands available.\n",
        "!sens\n", "!shutdown\n"}; //Dunno if I'll keep this or not yet, we'll see, might be easier with List?

        List<string> bans = new List<string>();

        Dictionary<string, int> Games = new Dictionary<string, int>();

        /// <summary>
        /// Before Form1.Load() runs we try to connect.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            Connect();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            #region TwitchBotStuff
            //We can't put Connect(); here because Form1.Load is only called once
            //When the application initally starts.
            //"But we only need to connect once?" Yes but what if we get disconnected somehow?
            //It's better to leave it as its own function in case we need it.
            if (tcpClient.Connected)
            {
                //If we're connected to chat, Initialize the streams.
                sreader = new StreamReader(tcpClient.GetStream());
                swriter = new StreamWriter(tcpClient.GetStream());
                //TCP instances have their own stream
                //

                //Now that the streams are initalized with chat, we can send info.
                swriter.WriteLine("PASS " + pwd + Environment.NewLine + "NICK " +
                    uname + Environment.NewLine + "USER " + uname + " 8 * :" + uname);
                //Now we've connected my account to the bot
                //Let's join my chat now
                swriter.WriteLine($"JOIN #{cName}");
                swriter.WriteLine("CAP REQ :twitch.tv/membership");
                swriter.WriteLine("CAP REQ :twitch.tv/tags");
                swriter.WriteLine("CAP REQ :twitch.tv/commands");
                swriter.WriteLine($"CAP REQ :twitch.tv/NAMES");
                swriter.Flush();
            }
            else
            {
                Connect();
            }
            #endregion
            //Do we want to make this application a YoutubeBot as well?
        }

        /// <summary>
        /// Connect to Twitch IRC
        /// </summary>
        private void Connect()
        {
            //By initializing the new TCP with a host and port, we connect
            // without having to use the .Connect() method provided.
            tcpClient = new TcpClient("irc.chat.twitch.tv", 6667);
            clabel.Text = "Bot connected.";
        }

        //Using a timer not just to nerf spam but to have a function that is called forever
        //Otherwise we'd have to  while(true){ Update();)
        //Then put the chat logging in the Update method
        private void timer1_Tick(object sender, EventArgs e)
        {
            //Make sure we're receiving information
            if (tcpClient.Available > 0 || sreader.Peek() >= 0)
            {
                //If we're receiving more than 0 bytes of data
                var msg = sreader.ReadLine();
                //Parse the string to get the User name of the sender
                //Remember 'msg' is the same as ":{uname}!{uname}@{uname}.tmi.twitch.tv PRIVMSG #{uname} :"
                //So we need to split it up
                if (msg.Contains("PRIVMSG"))//If a user sent a msg in chat
                {
                    string[] msg1 = msg.Split('!');
                    string msg2 = msg1[0]; // ':uname'! <-
                    string[] msgMsg = msg.Split(':');
                    string msg3 = msgMsg[2];  // :uname.......:'msg' <-
                    senderr = msg2.Remove(0, 1);//Takes away the ":" before the uname
                    clabel.Text += $"\r\n{msg2.Remove(0,1)}:{msg3}"; //Uname:Msg :D
                    if (msg3.StartsWith("!"))//If the message is a command
                    {
                        Commands(msg3, senderr); //Method to do cmds
                    }
                }
                else
                {
                    clabel.Text += $"\n{msg}";
                }
            }
        }

        /// <summary>
        /// Processes commands from chat.
        /// </summary>
        /// <param name="cmd">Command</param>
        /// <param name="se">Sender</param>
        private void Commands(string cmd, string sender)
        {
            if (cmd == "!help" || cmd == "!cmds" || cmd == "!commands")
                SendMessage(cmds);
            if (cmd == "!sens")
                SendMessage("400 DPI, 10 In Game(Overwatch)", sender);
            if (cmd.StartsWith("!ban"))
            {
                //Tbh kinda surprised this worked first try
                string[] ban = cmd.Split(' ');
                string user = ban[1];
                SendMessage($"/ban {user}");
                bans.Add(user);
                SendMessage(user + " was banned by a mod.");
                clabel.Text += Environment.NewLine + user + " was banned.";
            }
            if (cmd.StartsWith("!unban"))
            {
                string[] ban = cmd.Split(' ');
                string user = ban[1];
                SendMessage($"/unban {user}");
                SendMessage(user + " was unbanned by a mod.");
                bans.Remove(user);
                clabel.Text += Environment.NewLine + user + " was unbanned.";
            }
            if (cmd == "!list") //Display contents of the Banlist
            {
                if (bans.Count > 0)
                {
                    foreach (var bannedNigga in bans)
                    {
                        int i = 1;
                        SendMessage(i + "." + bannedNigga);
                        i++;
                    }
                }
                else
                {
                    SendMessage("Banlist: Vacant");
                }
            }
            if (cmd.StartsWith("!to"))//Timeout user
            {
                //Still need to implement [1]User and [2]Time
                //Lets limit it to 10 minutes for now.
                string[] ban = cmd.Split(' ');
                string user = ban[1];
                SendMessage($"/timeout {user}");
                SendMessage(user + " was timed out by a mod.");
            }
            if (cmd.StartsWith("!vote"))
            {
                //Doing a !vote_type kind of command
                //ex:!vote ban robbie
                //ex2:!vote timeout robbie 20seconds
                //ex3:!vote game overwatch
                //ex4:!vote clear (to clear chat)
                //Maybe use switches? Idk, still hard enough getting the ban to count
                List<string> senders = new List<string>();//Senders
                Dictionary<string, int> vBans = new Dictionary<string, int>();//User/int
                Dictionary<string, int> vTos = new Dictionary<string, int>();
                string[] vote = cmd.Split(' ');
                string type = vote[1]; //Type of vote
                if (type == "ban")//" ", !vote = 0, ban = 1, name = 2|| !vote ban name
                {
                    string ban = vote[2]; 
                    if (senders.Contains(sender))
                    {
                        SendMessage("You already voted on this ban, " + sender);
                    }
                    else 
                    {
                        senders.Add(sender); //Added sender
                        if (!vBans.ContainsKey(ban))
                        {
                            SendMessage("Vote added, " + sender);
                            vBans.Add(ban, 1); //Added user to be banned + num
                        }
                        else
                        {
                            vBans[ban]++;
                            SendMessage("A vote on " + ban + " has already been started, " + sender);
                            SendMessage("Total votes for " + ban + " are:" + vBans[ban]);
                        }
                    }

                }
                if (type == "clear")
                {

                }
                if (type == "timeout")
                {
                    string to = vote[2]; //User
                }
                if (type == "game")
                {
                    string gameWanted = vote[2];//Game
                    string str = gameWanted.ToLower();//game name to go in dict
                }
            }

        }

        /// <summary>
        /// To Send a message through the stream into chat.
        /// </summary>
        /// <param name="str">Message</param>
        /// <param name="se">Sender</param>
        void SendMessage(string str, string se) //To send a regualr message
        {
            //Read the overload below
            chatInfo = $":{uname}!{uname}@{uname}.tmi.twitch.tv PRIVMSG #{uname} :";
            chatInfo = chatInfo.ToLower();
            swriter.WriteLine($"{chatInfo}@{se} {str}");
            swriter.Flush();
        }

        void SendMessage(string str) //To send a regular message
        {
            //Read the overload below
            chatInfo = $":{uname}!{uname}@{uname}.tmi.twitch.tv PRIVMSG #{uname} :";
            chatInfo = chatInfo.ToLower();
            swriter.WriteLine($"{chatInfo} {str}");
            swriter.Flush();
        }

        /// <summary>
        /// To send the array of Commands
        /// </summary>
        /// <param name="strAr"></param>
        void SendMessage(string[] strAr)//To send the array of commands
        {
            //The only way to send messages to chat is with this
            chatInfo = $":{uname}!{uname}@{uname}.tmi.twitch.tv PRIVMSG #{uname} :";
            chatInfo = chatInfo.ToLower(); //Just making sure nothing is capitalized
            for (int i = 0; i < strAr.Length; i++)
            {
                //Looping through the whole array and outputting everything inside
                swriter.WriteLine($"{chatInfo} {strAr[i]}");
            }
            //Always remember to clear the stream writers to clear buffers and to make sure
            // any data is pushed onto the stream.
            swriter.Flush();
        }

        /// <summary>
        /// To send the message from the bot side by hitting Enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void botChatBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (botChatBox.Text != null)
                {
                    if (!string.IsNullOrEmpty(botChatBox.Text) && !botChatBox.Text.StartsWith("!"))
                    {
                        SendMessage($"{botChatBox.Text}");
                        botChatBox.Text = string.Empty;//Clears the text
                        e.SuppressKeyPress = true; //NO MORE STUPID FUCKING DING SOUND
                    }
                    else if (botChatBox.Text.StartsWith("!"))
                    {
                        {
                            if (botChatBox.Text == "!ping")
                                clabel.Text += Environment.NewLine + "Functioning properly.";
                            if (botChatBox.Text == "!shutdown")
                                Application.Exit();
                            if (botChatBox.Text == "!pls")
                            {
                               
                            }
                        }
                        botChatBox.Text = "";
                        e.SuppressKeyPress = true;
                    }
                }
            }
        }
    }
}
