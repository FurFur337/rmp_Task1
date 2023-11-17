using RAGE;
using RAGE.Elements;

public class Client : Events.Script
{
    public Client()
    {
        Events.Add("SERVER:CLIENT::OnWantedLevelUpdate", WantedSystem.OnWantedLevelUpdate);
        Events.Add("SERVER:CLIENT::OnBlipSetupForCop", WantedSystem.OnBlipSetupForCop);
        Events.Add("SERVER:CLIENT::OnBlipRemoveForCop", WantedSystem.OnBlipRemoveForCop);
    }
}