#Include "MathLib" as ML
#Include "TextLib" as TL

Void updateCountdownText(Integer value) {
    declare countdownText <=> (Page.MainFrame.GetFirstChild("countdowntext") as CMlLabel);
    declare plural = "s";
    if (value == 1) {
        plural = "";
    }

    if (value < 1) {
        countdownText.SetText("You are about to be kicked.");
        TriggerPageAction("");
    } else {
        countdownText.SetText("You will be automatically kicked in " ^ value ^ " second" ^ plural ^ ".");
    }
}

main() {
    declare Integer countdown = {{KickTimeout}};
    declare lastUpdate = Now;

    updateCountdownText(countdown);

    while(True) {
        yield;

        if (Now - lastUpdate > 1000 && countdown > 0) {
            countdown -= 1;
            lastUpdate = Now;
            updateCountdownText(countdown);
        }
    }
}