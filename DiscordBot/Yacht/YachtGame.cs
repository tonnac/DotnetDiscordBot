using DisCatSharp;
using DisCatSharp.Common;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;

namespace DiscordBot.Yacht;

enum EYachtPointType : ushort 
{
Aces,
Deuces,
Threes,
Fours,
Fives,
Sixes,
Choice,
FourOfaKind,
FullHouse,
SStraight,
LStraight,
Yacht,
SubTotal,
Bonus,
Total,
}

public class YachtGame
{
    public DiscordUser? _1P;
    public DiscordUser? _2P;
    public DiscordThreadChannel? _yachtChannel;
    public DiscordMessage? _yachtScoreUiMessage;
    public DiscordMessage? _yachtDiceTrayUiMessage;
    private readonly List<DiscordMessage> _prevUiMessages = new List<DiscordMessage>();

    private int _turn = 1;

    private readonly int[,] _points = new int[2, 13];
    private readonly int[] _tempPoints = new int[12];
    private readonly int[] _dices = new int[5];

    private readonly List<int> _diceTarget = new List<int>(5);


    public DiscordUser? CurrPlayer => _turn % 2 == 0 ? _2P : _1P;
    public string? TurnName => _turn % 2 == 0 ? "2P" : "1P";

    public async Task ClearUi()
    {
        await _yachtChannel?.DeleteMessagesAsync(_prevUiMessages)!;
        _prevUiMessages.Clear();
    }
    public async Task  SetUi(DiscordMessage diceTray, DiscordMessage scoreUi)
    {
        if (_yachtDiceTrayUiMessage != null) _prevUiMessages.Add(_yachtDiceTrayUiMessage);
        if (_yachtScoreUiMessage != null) _prevUiMessages.Add(_yachtScoreUiMessage);

        _yachtDiceTrayUiMessage = diceTray;
        _yachtScoreUiMessage = scoreUi;
    }

    public static string DiceFaceEmojiCode(int diceNum)
    {
        switch (diceNum)
        {
           default: 
           case 1 : return "\u2680"; 
           case 2 : return "\u2681"; 
           case 3 : return "\u2682"; 
           case 4 : return "\u2683"; 
           case 5 : return "\u2684"; 
           case 6 : return "\u2685"; 
        }
    }

    private void PointTempSettle()
    {
        for (int i = 0; i < _tempPoints.Length; i++)
        {
            _tempPoints[i] = 0;
        }

        int choice = 0;

        foreach (var dice in _dices)
        {
            choice += dice;
        }

        _tempPoints[(int)EYachtPointType.Choice] = choice;

        int[] fullHouseCheck = new int[6];

        for (int i = 0; i < 6; i++)
        {
            int currCheckDiceNum = i + 1;
            int count = 0;
            for (int j = 0; j < 5; j++)
            {
                if (_dices[j] == currCheckDiceNum)
                    count++;
            }
            fullHouseCheck[i] = count;

            _tempPoints[i] = count * (i + 1);

            if (count >= 4)
            {
                _tempPoints[(int) EYachtPointType.FourOfaKind] = choice;
            }

            if (count >= 5)
            {
                _tempPoints[(int) EYachtPointType.Yacht] = 50;
                _tempPoints[(int) EYachtPointType.FullHouse] = choice;
            }

        }

        bool two = false;
        bool three = false;

        for (int i = 0; i < 6; i++)
        {
            if (fullHouseCheck[i] == 3)
                three = true;

            if (fullHouseCheck[i] == 2)
                two = true;
        }

        if (two && three)
        {
            _tempPoints[(int)EYachtPointType.FullHouse] = choice;
        }

        List<int> sortList = new List<int>();

        sortList.AddRange(_dices);
        sortList.Sort();

        int straightCheck = 0;

        for (int i = 0; i < 4; i++)
        {
            if (sortList[i] + 1 != sortList[i + 1])
            {
                straightCheck++;
            }

        }

        if (straightCheck == 0)
            _tempPoints[(int)EYachtPointType.LStraight] = 30;

        for (int i = 0; i < 3; i++)
        {
            int count = 0;
            for (int j = 0; j < 4; j++)
            {
                if (fullHouseCheck[j + i] > 0)
                    count++;
            }

            if (count >= 4)
                _tempPoints[(int)EYachtPointType.SStraight] = 15;

        }
    }
    private void PointSettle()
    {
        int topSum = 0;
        int turn = _turn % 2;
        for (int i = 0; i < 6; i++)
        {
            topSum += _points[turn,i];
        }

        _points[turn,(int)EYachtPointType.SubTotal] = topSum;
        if (topSum >= 63)
        {
            _points[turn,(int)EYachtPointType.Bonus] = 35;
        }
        int totalSum = 0;
        for (int i = 0; i < 13; i++)
        {
            totalSum += _points[turn,i];
        }
        _points[turn,(int)EYachtPointType.Total] = totalSum;
       
    }
    public void GameSettle()
    {

    }
    public async Task DiceTrayMessageReactionAdded(DiscordClient client, MessageReactionAddEventArgs eventArgs)
    {
        if (_yachtChannel!.Id != eventArgs.ChannelId)
            return;
        
        if (eventArgs.Message.Id != _yachtDiceTrayUiMessage!.Id)
            return;
            
        if (CurrPlayer?.Id != eventArgs.Message.Author.Id)
            return;

        switch (eventArgs.Emoji.Name)
        {
            case "1️⃣": break;
            case "2️⃣": break;
            case "3️⃣": break;
            case "4️⃣": break;
            case "5️⃣": break;
            case "6️⃣": break;
        }
  

        DiscordEmoji.FromName(client,"dice1");
        DiscordEmoji.FromName(client,"dice2");
        DiscordEmoji.FromName(client,"dice3");
        DiscordEmoji.FromName(client,"dice4");
        DiscordEmoji.FromName(client,"dice5");
        DiscordEmoji.FromName(client,"dice6");
        
        
    }

    public async Task ScoreBoardMessageReactionAdded(DiscordClient client, MessageReactionAddEventArgs eventArgs)
    {
        if (_yachtChannel!.Id != eventArgs.ChannelId)
            return;
        
        if (eventArgs.Message.Id != _yachtScoreUiMessage!.Id)
            return;

        if (CurrPlayer?.Id != eventArgs.Message.Author.Id)
            return;


    }

    void DiceRoll()
    {
        Random rand = new Random();
        for (int i = 0 ; i<_dices.Length;i++)
        {
            if (!_diceTarget.EmptyOrNull())
            {
                if (!_diceTarget.Contains(i))
                    continue;
            }
            _dices[i] = rand.Next(1, 7);
        }
    }

    void DiceReset()
    {
        _diceTarget.Clear();
        for (int i = 0; i < _dices.Length; i++)
        {
            _dices[i] = 0;
        }
    }

    public DiscordEmbedBuilder ScoreBoardVisualize()
    {
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Orange)
            .WithTitle("ScoreBoard");
        return embedBuilder;
    }

     public DiscordEmbedBuilder DiceTrayVisualize()
     {
         DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
             .WithColor(DiscordColor.Green)
             .WithTitle(TurnName + " : " + CurrPlayer?.Username);

         foreach (var diceNum in _dices)
         {
             embedBuilder.AddField(new DiscordEmbedField(diceNum.ToString(),DiceFaceEmojiCode(diceNum),true));
         }

         return embedBuilder;
     }
}