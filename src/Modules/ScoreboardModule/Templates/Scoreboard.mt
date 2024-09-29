﻿<component>
    <using namespace="EvoSC.Modules.Official.ScoreboardModule.Config"/>
    <using namespace="System.Linq"/>

    <import component="ScoreboardModule.Components.BackgroundBox" as="ScoreboardBackground"/>
    <import component="ScoreboardModule.Components.PlayerRow.Framemodel" as="PlayerRowFramemodel"/>
    <import component="ScoreboardModule.Components.Settings.Wrapper" as="SettingsWrapper"/>
    <import component="ScoreboardModule.Components.Settings.Form" as="SettingsForm"/>
    <import component="ScoreboardModule.ComponentsNew.ScoreboardHeader" as="Header"/>
    <import component="ScoreboardModule.ComponentsNew.ScoreboardBody" as="Body"/>
    <import component="ScoreboardModule.ComponentsNew.ScoreboardBg" as="ScoreboardBg"/>
    <import component="EvoSC.Style.UIStyle" as="UIStyle" />

    <property type="IScoreboardSettings" name="settings"/>
    <property type="int" name="MaxPlayers" default="0"/>
    <property type="int" name="VisiblePlayers" default="8"/>
    <property type="Dictionary<int, string>" name="PositionColors"/>

    <property type="double" name="backgroundBorderRadius" default="3.0"/>
    <property type="double" name="headerHeight" default="17.0"/>
    <property type="double" name="rowHeight" default="8.0"/>
    <property type="double" name="rowInnerHeight" default="5.0"/>
    <property type="double" name="rowSpacing" default="0.3"/>
    <property type="double" name="columnSpacing" default="4.0" />
    <property type="double" name="pointsWidth" default="16.0"/>
    <property type="double" name="padding" default="2.0"/>
    <property type="double" name="innerSpacing" default="1.6"/>
    <property type="double" name="legendHeight" default="3.8"/>
    <property type="int" name="actionButtonCount" default="2"/>

    <template layer="ScoresTable_x">
        <UIStyle />
        <frame pos="{{ settings.Width / -2.0 }} {{ settings.Height / 2.0 }}">
            <ScoreboardBg width="{{ settings.Width }}"
                          height="{{ settings.Height }}"
            />
            <Header width="{{ settings.Width }}"
                    height="{{ headerHeight }}"
            />
            <Body y="{{ -headerHeight }}"
                  width="{{ settings.Width }}"
                  height="{{ settings.Height - headerHeight }}"
                  legendHeight="{{ legendHeight }}"
                  rowSpacing="{{ rowSpacing }}"
                  columnSpacing="{{ columnSpacing }}"
                  flagWidth="{{ rowInnerHeight * 1.5 }}"
                  clubTagWidth="{{ rowInnerHeight * 1.5 }}"
            />

            <!-- Player Rows -->
            <frame id="rows_wrapper" pos="0 {{ -headerHeight-legendHeight }}" size="{{ settings.Width }} {{ settings.Height-headerHeight }}">
                <frame id="rows_inner">
                    <!-- Player Rows -->
                    <PlayerRowFramemodel w="{{ settings.Width }}"
                                         padding="{{ padding }}"
                                         rowHeight="{{ rowHeight }}"
                                         rowSpacing="{{ rowSpacing }}"
                                         columnSpacing="{{ columnSpacing }}"
                                         innerSpacing="{{ innerSpacing }}"
                                         rowInnerHeight="{{ rowInnerHeight }}"
                                         pointsWidth="{{ pointsWidth }}"
                                         actionButtonCount="{{ actionButtonCount }}"
                    />
                    <frame id="frame_scroll"
                           size="{{ settings.Width }} {{ VisiblePlayers * rowHeight * rowSpacing + headerHeight - legendHeight + rowSpacing }}">
                        <frameinstance modelid="player_row"
                                       foreach="int rowId in Enumerable.Range(0, MaxPlayers * 2).ToList()"
                                       pos="0 {{ rowId * -rowHeight + (rowId+1) * -rowSpacing }}"
                        />
                    </frame>
                </frame>
            </frame>
        </frame>
    </template>

    <script>
        <!--
        #Include "MathLib" as ML
        #Include "TextLib" as TL
        #Include "ColorLib" as CL
        #Include "Libs/Nadeo/TMGame/Modes/MV_Utils.Script.txt" as MV_Utils
        #Include "Libs/Nadeo/TMGame/Modes/Base/UIModules/ScoresTable_Common.Script.txt" as UIModules_ScoresTable
        #Include "Libs/Nadeo/TMGame/Modes/Base/UIModules/Helpers_Client.Script.txt" as RaceHelpers
    
        #Const C_Status_Disconnected	0
        #Const C_Status_Spawned			1
        #Const C_Status_NotSpawned		2
        #Const C_Status_Spectating		3

        #Const UIModules_ScoresTable::C_Mode_BestTime as C_Mode_BestTime
        #Const UIModules_ScoresTable::C_Mode_PrevTime as C_Mode_PrevTime
        #Const UIModules_ScoresTable::C_Mode_LapTime as C_Mode_LapTime
        #Const UIModules_ScoresTable::C_Mode_Points as C_Mode_Points
        #Const UIModules_ScoresTable::C_Mode_Laps as C_Mode_Laps
        #Const UIModules_ScoresTable::C_Mode_Trophy as C_Mode_Trophy
        #Const UIModules_ScoresTable::C_Mode_RaceProgression as C_Mode_RaceProgression

        #Const C_CustomPoints_Text 0
        #Const C_CustomPoints_Color 1
        
        declare Integer CurrentScoreMode;
        declare Integer PlayerRowsVisible;
        declare Integer PlayerRowsFilled;
        declare Integer ScrollIndex;
        declare Integer MaxPlayers;
        declare CMlFrame RowsFrame;
        declare Text[Integer] PositionColors;
        
        Boolean ShouldShowPointsBox() {
            return CurrentScoreMode == C_Mode_LapTime
                || CurrentScoreMode == C_Mode_Laps
                || CurrentScoreMode == C_Mode_Points;
        }
        
        Void UpdateScoreboardLayout() {
            declare persistent Boolean SB_Setting_ShowClubTags for LocalUser = True;
            declare persistent Boolean SB_Setting_ShowFlags for LocalUser = True;
            declare shouldShowPoints = ShouldShowPointsBox();
            declare Real flagWidth = {{ rowInnerHeight }} * 2.0;
            declare Real innerSpacing = {{ innerSpacing }} * 1.0;
            
            foreach(Control in RowsFrame.Controls){
                declare Real offset = 0.0;
                declare playerRow = (Control as CMlFrame);
                declare pointsBoxFrame = (playerRow.GetFirstChild("points_box") as CMlFrame);
                declare flagQuad = (playerRow.GetFirstChild("flag") as CMlQuad);
                declare clubLabel = (playerRow.GetFirstChild("club") as CMlLabel);
                declare nameLabel = (playerRow.GetFirstChild("name") as CMlLabel);
                declare pointsLabel = (playerRow.GetFirstChild("points") as CMlLabel);
                
                pointsBoxFrame.Visible = shouldShowPoints;
                pointsLabel.Visible = shouldShowPoints;
            }
        }
        
        Text StripLeadingZeroes(Text input) {
            return TL::RegexReplace("^[0.:]+", input, "", "");
        }

        Integer[CSmScore] GetSortedScores() {
            declare persistent Boolean[Text] LibScoresTable2_Settings for This;
            declare SortedScores = Integer[CSmScore];
            declare c = 0;
    
            foreach (Score in Scores) {
                declare LibST_Status for Score.User = C_Status_Disconnected;
                declare Weight = c;
                if (
                    LibScoresTable2_Settings.existskey("SortSpec") &&
                    LibScoresTable2_Settings["SortSpec"] &&
                    LibST_Status == C_Status_Spectating
                ) Weight += Scores.count;
                if (
                    LibScoresTable2_Settings.existskey("SortDisco") &&
                    LibScoresTable2_Settings["SortDisco"] &&
                    LibST_Status == C_Status_Disconnected
                ) Weight += 2 * Scores.count;
                SortedScores[Score] = Weight;
                c += 1;
            }
    
            return SortedScores.sort();
        }
        
        Void SetCountryFlag(CMlQuad flagQuad, Text login){
            if(login != "" && !TL::StartsWith("*fakeplayer", login)){
                flagQuad.ImageUrl = "file://ZoneFlags/Login/" ^ login ^ "/country";
                flagQuad.Opacity = 1.0;
            }else{
                flagQuad.ImageUrl = "file://ZoneFlags/World";
                flagQuad.Opacity = 1.0;
                //flagQuad.BgColor = <0.0, 0.0, 0.0>;
                //flagQuad.Opacity = 0.15;
            }
        }
        
        Void SetCustomLabel(CMlFrame playerRow, Text value, Text hexColor){
            declare customLabel = (playerRow.GetFirstChild("custom_label") as CMlLabel);
            //declare customGradientFrame = (playerRow.GetFirstChild("custom_gradient") as CMlFrame);
            
            customLabel.Value = value;
            customLabel.TextColor = CL::HexToRgb(hexColor);
            
            /*
            Page.GetClassChildren("modulate", customGradientFrame, True);
            foreach(Control in Page.GetClassChildren_Result){
                (Control as CMlQuad).ModulateColor = customLabel.TextColor;
            }
            Page.GetClassChildren("set", customGradientFrame, True);
            foreach(Control in Page.GetClassChildren_Result){
                (Control as CMlQuad).BgColor = customLabel.TextColor;
            }
            
            customGradientFrame.Show();
            */
        }
        
        Void HideCustomLabel(CMlFrame playerRow){
            declare customLabel = (playerRow.GetFirstChild("custom_label") as CMlLabel);
            //declare customGradientFrame = (playerRow.GetFirstChild("custom_gradient") as CMlFrame);
            customLabel.Value = "";
            //customGradientFrame.Hide();
        }
        
        Void UpdateScoreAndPoints(CSmScore Score, CMlFrame playerRow, Integer position){
            if(Score == Null || Score.User == Null || playerRow == Null){
                return;
            }
            
            declare netread Text[][Text] Net_TMGame_ScoresTable_CustomPoints for Teams[0];
            declare netread Integer[Text] Net_TMxSM_ScoresTable_CustomTimes for Teams[0];
            declare Boolean CustomPointsEnabled = Net_TMGame_ScoresTable_CustomPoints.existskey(Score.User.WebServicesUserId);
            declare Boolean Race_ScoresTable_IsSpectator for Score = False;
            declare ScoresTable_PlayerLastUpdate for Score = -1;
            declare Boolean PlayerIsConnected = ScoresTable_PlayerLastUpdate == Now;
            declare Boolean colorizePosition = False;
            declare CSmScore playerScore for playerRow;
            playerScore <=> Score;
                
            declare scoreLabel = (playerRow.GetFirstChild("score") as CMlLabel);
            declare specDisconnectedLabel = (playerRow.GetFirstChild("spec_disconnected_label") as CMlLabel);
            declare pointsLabel = (playerRow.GetFirstChild("points") as CMlLabel);
            declare roundPointsLabel = (playerRow.GetFirstChild("round_points") as CMlLabel);
            declare customLabel = (playerRow.GetFirstChild("custom_label") as CMlLabel);
            
            if (!(CustomPointsEnabled && CurrentScoreMode != C_Mode_Trophy)) {
                HideCustomLabel(playerRow);
            }
            
            if (CustomPointsEnabled && CurrentScoreMode != C_Mode_Trophy) {
                declare CustomLabelColorHex = "000";
                declare CustomLabelValue = "";
                
                if (Net_TMGame_ScoresTable_CustomPoints[Score.User.WebServicesUserId].existskey(C_CustomPoints_Text)) {
                    CustomLabelValue = Net_TMGame_ScoresTable_CustomPoints[Score.User.WebServicesUserId][C_CustomPoints_Text];
                }else{
                    CustomLabelValue = "";
                }
                if (Net_TMGame_ScoresTable_CustomPoints[Score.User.WebServicesUserId].existskey(C_CustomPoints_Color)) {
                    CustomLabelColorHex = Net_TMGame_ScoresTable_CustomPoints[Score.User.WebServicesUserId][C_CustomPoints_Color];
                }
            
                if(CustomLabelValue != ""){
                    SetCustomLabel(playerRow, CustomLabelValue, CustomLabelColorHex);
                }else{
                    HideCustomLabel(playerRow);
                }
            } else if (CurrentScoreMode == C_Mode_Points) {
                customLabel.Value = "";
                pointsLabel.Value = TL::ToText(Score.Points);
                colorizePosition = Score.Points > 0;
                
                if(Score.PrevRaceTimes.count > 0 && Score.PrevRaceTimes[Score.PrevRaceTimes.count - 1] > 0){
                    scoreLabel.Value = TL::TimeToText(Score.PrevRaceTimes[Score.PrevRaceTimes.count - 1], True, True);
                }else{
                    declare CSmPlayer::ESpawnStatus Race_ScoresTable_SpawnStatus for Score = CSmPlayer::ESpawnStatus::NotSpawned;
                    if(Race_ScoresTable_SpawnStatus == CSmPlayer::ESpawnStatus::NotSpawned && PlayerIsConnected && !Race_ScoresTable_IsSpectator){
                        scoreLabel.Value = "DNF";
                    }else{
                        scoreLabel.Value = "";
                    }
                }
                
                if(Score.RoundPoints != 0){
                    declare sign = "+";
                    if(Score.RoundPoints < 0){
                        sign = "-";
                    }
                    roundPointsLabel.Value = sign ^ Score.RoundPoints;
                }else{
                    roundPointsLabel.Value = "";
                }
            } else if (Net_TMxSM_ScoresTable_CustomTimes.existskey(Score.User.WebServicesUserId)) {
                customLabel.Value = "";
                if (Net_TMxSM_ScoresTable_CustomTimes[Score.User.WebServicesUserId] > 0){
                    scoreLabel.Value = TL::TimeToText(Net_TMxSM_ScoresTable_CustomTimes[Score.User.WebServicesUserId], True, True);
                    colorizePosition = True;
                } else {
                    scoreLabel.Value = "nope";
                }
            } else if (CurrentScoreMode == C_Mode_BestTime && Score.BestRaceTimes.count > 0) {
                customLabel.Value = "";
                declare bestRaceTime = Score.BestRaceTimes[Score.BestRaceTimes.count - 1];
                scoreLabel.Value = TL::TimeToText(bestRaceTime, True, True);
                colorizePosition = bestRaceTime > 0;
            } else if (CurrentScoreMode == C_Mode_PrevTime && Score.PrevRaceTimes.count > 0) {
                customLabel.Value = "";
                declare bestPrevTime = Score.PrevRaceTimes[Score.PrevRaceTimes.count - 1];
                scoreLabel.Value = TL::TimeToText(bestPrevTime, True, True);
                colorizePosition = bestPrevTime > 0;
            } else if (CurrentScoreMode == C_Mode_LapTime && Score.BestLapTimes.count > 0) {
                customLabel.Value = "";
                scoreLabel.Value = TL::TimeToText(Score.BestLapTimes[Score.BestLapTimes.count - 1], True, True);
            } else if (CurrentScoreMode == C_Mode_Laps && Score.BestRaceTimes.count > 0) {
                customLabel.Value = "";
                scoreLabel.Value = TL::TimeToText(Score.BestRaceTimes[Score.BestRaceTimes.count - 1], True, True);
                pointsLabel.Value = ""^Score.BestRaceTimes.count;
            } else if (CurrentScoreMode == C_Mode_RaceProgression) {
                customLabel.Value = "";
                declare netread Int2 Net_TMxSM_ScoresTable_RaceProgression for Score;
                pointsLabel.Value = ""^Net_TMxSM_ScoresTable_RaceProgression.X;
                if (Net_TMxSM_ScoresTable_RaceProgression.Y > 0) {
                    scoreLabel.Value = TL::TimeToText(Net_TMxSM_ScoresTable_RaceProgression.Y, True, True);
                    colorizePosition = True;
                } else {
                    scoreLabel.Value = "0:00.000";
                }
            } else if (CurrentScoreMode == C_Mode_Trophy) {
                customLabel.Value = "";
                scoreLabel.Value = "0:00.000"; //todo: implement trophy mode
            } else {
                customLabel.Value = "";
                scoreLabel.Value = "0:00.000";
            }
            
            scoreLabel.Value = StripLeadingZeroes(scoreLabel.Value);
            
            declare positionBox = (playerRow.GetFirstChild("position_box") as CMlFrame);
            declare playerRowBg = (playerRow.GetFirstChild("player_row_bg") as CMlFrame);
            if(PositionColors.existskey(position) && colorizePosition){
                declare positionColor = PositionColors[position];
                SetPositionBoxColor(positionBox, CL::HexToRgb(positionColor));
                //SetPlayerHighlightColor(playerRowBg, CL::HexToRgb(positionColor));
            }else{
                SetPositionBoxColor(positionBox, CL::HexToRgb("{{ Theme.UI_AccentPrimary }}"));
                //SetPlayerHighlightColor(playerRowBg, CL::HexToRgb("{{ Theme.UI_AccentPrimary }}"));
            }
            
            if (PlayerIsConnected) {
                //connected
                if(Race_ScoresTable_IsSpectator){
                    specDisconnectedLabel.Value = "";
                }else{
                    specDisconnectedLabel.Value = "";
                }
            }else{
                //disconnected
                specDisconnectedLabel.Value = "";
            }
            
            //align items
            declare offset = 0.0;
            declare x = scoreLabel.RelativePosition_V3.X;
            
            if(scoreLabel.Value != ""){
                offset += scoreLabel.ComputeWidth(scoreLabel.Value) + {{ innerSpacing }};
            }
            customLabel.RelativePosition_V3.X = x - offset;
            if(customLabel.Value != ""){
                offset += customLabel.ComputeWidth(customLabel.Value) + {{ innerSpacing }};
            }
            roundPointsLabel.RelativePosition_V3.X = x - offset;
            if(roundPointsLabel.Value != ""){
                offset += roundPointsLabel.ComputeWidth(roundPointsLabel.Value) + {{ innerSpacing }};
            }
            specDisconnectedLabel.RelativePosition_V3.X = x - offset;
        }
        
        Text GetRecordText() {
            declare Integer SB_PointsLimit for UI = -2;
            
            if(SB_PointsLimit >= 0){
                if(SB_PointsLimit == 0){
                    return "POINTS LIMIT: UNLIMITED";
                }
                
                return "POINTS LIMIT: " ^ SB_PointsLimit;
            }
        
            return "AUTHOR TIME | " ^ TL::TimeToText(Map.TMObjective_AuthorTime, True, True);
        }
        
        Void UpdateHeaderInfo() {
            /*
            declare subTextLabel <=> (Page.MainFrame.GetFirstChild("sub_text") as CMlLabel);
            declare roundLabel <=> (Page.MainFrame.GetFirstChild("round_label") as CMlLabel);
            subTextLabel.Value = GetRecordText();
            declare Owner <=> MV_Utils::GetOwner(This);
            
            if (CurrentScoreMode == C_Mode_BestTime || CurrentScoreMode == C_Mode_PrevTime){
                declare timeLimit = RaceHelpers::GetTimeLimit(Teams[0]);
                roundLabel.Value = "TIME LIMIT | ";
                if(timeLimit <= 0){
                    roundLabel.Value ^= "UNLIMITED";
                }else{                   
                    roundLabel.Value ^= TL::TimeToText(timeLimit);
                }
            }else if (CurrentScoreMode == C_Mode_LapTime || CurrentScoreMode == C_Mode_Laps){
				declare Integer LapCurrent = -1;
				if(Owner != Null){
                    declare Integer LapCurrent = RaceHelpers::GetPlayerLap(Owner);
				}
				declare LapsTotal = RaceHelpers::GetLapsNb(Teams[0]);
                roundLabel.Value = TL::Compose("%1 | %2 OF %3", _("|Race|Lap"), TL::ToText(LapCurrent), TL::ToText(LapsTotal));
            }else if (CurrentScoreMode == C_Mode_Points) {
                declare Integer SB_CurrentRound for UI = 0;
                declare Integer SB_RoundsPerMap for UI = 0;
                roundLabel.Value = TL::Compose("ROUND | %1 OF %2", TL::ToText(SB_CurrentRound), TL::ToText(SB_RoundsPerMap));
            }else{
                roundLabel.Value = "";
            }
            */
        }
        
        Void UpdateScrollSize(Integer playerRowsFilled) {
            declare scrollN = 0;
            if(playerRowsFilled >= 8){
                scrollN = playerRowsFilled - 8;
            }
            RowsFrame.ScrollMax = <0.0, {{ rowHeight + rowSpacing }} * scrollN * 1.0>;
            PlayerRowsFilled = playerRowsFilled;
        }
        
        Text GetNickname(CUser user) {
            declare Text[Text] EvoSC_Player_Nicknames for UI = [];
            if(EvoSC_Player_Nicknames.existskey(user.Login)){
                return EvoSC_Player_Nicknames[user.Login];
            }
            
            return user.Name;
        }
        
        Void UpdateScoreTable() {
            UpdateHeaderInfo();
            
            foreach (PlayerIndex => Player in Players) {
                if (Player.Score == Null) continue;
                
                declare ScoresTable_PlayerLastUpdate for Player.Score = -1;
                ScoresTable_PlayerLastUpdate = Now;
                
                declare Boolean Race_ScoresTable_IsSpectator for Player.Score = False;
                Race_ScoresTable_IsSpectator = Player.RequestsSpectate;
                
                declare CSmPlayer::ESpawnStatus Race_ScoresTable_SpawnStatus for Player.Score = CSmPlayer::ESpawnStatus::NotSpawned;
                Race_ScoresTable_SpawnStatus = Player.SpawnStatus;
            }
            
            declare cursor = 0;
            //declare startFill = ML::Max(ScrollIndex - PlayerRowsVisible, 0);
            //declare endFill = ML::Min(ScrollIndex + PlayerRowsVisible * 2, MaxPlayers - 1);
           
            foreach(Score => Weight in GetSortedScores()){
                //if(cursor < startFill || cursor > endFill){
                //    cursor += 1;
                //    continue;
                //}
                
                declare persistent Boolean SB_Setting_ShowSpectators for LocalUser = True;
                declare persistent Boolean SB_Setting_ShowDisconnected for LocalUser = True;
                
                if(!SB_Setting_ShowSpectators){
                    declare Boolean Race_ScoresTable_IsSpectator for Score = False;
                    if(Race_ScoresTable_IsSpectator){
                        continue;
                    }
                }
                if(!SB_Setting_ShowDisconnected){
                    declare ScoresTable_PlayerLastUpdate for Score = -1;
                    declare Boolean PlayerIsConnected = ScoresTable_PlayerLastUpdate == Now;
                    if(!PlayerIsConnected){
                        continue;
                    }
                }
                
                declare playerRow = (RowsFrame.Controls[cursor] as CMlFrame);
                declare clubLabel = (playerRow.GetFirstChild("club") as CMlLabel);
                declare nameLabel = (playerRow.GetFirstChild("name") as CMlLabel);
                declare flagQuad = (playerRow.GetFirstChild("flag") as CMlQuad);
                declare scoreLabel = (playerRow.GetFirstChild("score") as CMlLabel);
                declare pointsBoxFrame = (playerRow.GetFirstChild("points_box") as CMlFrame);
                declare positionBoxFrame = (playerRow.GetFirstChild("position_box") as CMlFrame);
                
                SetPlayerRank(positionBoxFrame, cursor + 1);
                nameLabel.Value = GetNickname(Score.User);
                clubLabel.Value = Score.User.ClubTag;
                if(clubLabel.Value == ""){
                    clubLabel.Value = "-";
                }
                
                declare Boolean CustomLabelVisible for playerRow = False;
                declare Boolean RowIsLocked for playerRow = False;
                
                if(!RowIsLocked){
                    UpdateScoreAndPoints(Score, playerRow, cursor + 1);
                    SetCountryFlag(flagQuad, Score.User.Login);
                }
                
                if(ShouldShowPointsBox()){
                    scoreLabel.RelativePosition_V3.X = pointsBoxFrame.RelativePosition_V3.X - {{ innerSpacing }};
                    pointsBoxFrame.Show();
                }else{
                    scoreLabel.RelativePosition_V3.X = {{ settings.Width - padding - innerSpacing }};
                    pointsBoxFrame.Hide();
                }
                
                playerRow.Show();
                
                cursor += 1;
            }
            
            //Hide remaining rows
            for(i, cursor, {{ MaxPlayers - 1 }}){
                declare playerRow = (RowsFrame.Controls[i] as CMlFrame);
                playerRow.Hide();
            }
            UpdateScrollSize(cursor);
        }
        -->
    </script>

    <script>
        <!--
        *** OnInitialization ***
        ***
            declare netread Integer Net_TMGame_ScoresTable_ScoreMode for Teams[0];
            //declare Integer scoreboardUpdateInterval = 333;
            declare Integer scoreboardUpdateInterval = 25;
            declare Integer lastScoreboardUpdate = 0;
            RowsFrame <=> (Page.MainFrame.GetFirstChild("frame_scroll") as CMlFrame);
            
            RowsFrame.ScrollActive = True;
            RowsFrame.DisablePreload = True;
            RowsFrame.ScrollGridSnap = True;
            RowsFrame.ScrollMin = <0.0, 0.0>;
            RowsFrame.ScrollMax = <0.0, {{ MaxPlayers * (rowHeight + rowSpacing) - settings.Height }} * 1.0>;
            RowsFrame.ScrollGrid = <0.0, {{ rowHeight + rowSpacing }} * 1.0>;
            
            MaxPlayers = {{ MaxPlayers }};
            PlayerRowsVisible = {{ VisiblePlayers }};
            PlayerRowsFilled = -1;
            CurrentScoreMode = -1;
            
            {! string.Join("\n", PositionColors.Select(pc => $"PositionColors[{pc.Key}] = \"{pc.Value}\";")) !}
        ***
        
        *** OnLoop *** 
        ***
            if(lastScoreboardUpdate + scoreboardUpdateInterval < Now){
                ScrollIndex = ML::NearestInteger(RowsFrame.ScrollOffset.Y / {{ rowHeight + rowSpacing }});
                UpdateScoreTable();
                lastScoreboardUpdate = Now;
            }
            
            if(CurrentScoreMode != Net_TMGame_ScoresTable_ScoreMode){
                CurrentScoreMode = Net_TMGame_ScoresTable_ScoreMode;
                UpdateScoreboardLayout();
                log("[EvoSC#] Update scoreboard layout.");
            }
        ***
        
        *** OnScriptExecutionFinished *** 
        ***
            sleep(5000);
            TriggerPageAction("ScoreboardManialinkController/ResendScoreboard");
        ***
        -->
    </script>

    <script resource="EvoSC.Scripts.UIScripts"/>
</component>
