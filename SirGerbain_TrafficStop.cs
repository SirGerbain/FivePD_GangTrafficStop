using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using FivePD.API;
using System.Collections.Generic;
using CitizenFX.Core.Native;

namespace SirGerbain_TrafficStop
{
    [CalloutProperties("Gang Traffic Stop", "sirGerbain", "1.0.0")]
    public class SirGerbain_TrafficStop : FivePD.API.Callout
    {
        Random random = new Random();

        Ped[] EastSideBallaGang, GroveStreetFamilies;
        Ped npcOfficer, npcGangMember;
        Vehicle npcPoliceCar, npcGangMemberCar;
        List<WeaponHash> weaponHashList = new List<WeaponHash>();
        List<VehicleHash> carHashList = new List<VehicleHash>();
        List<PedHash> esbHashList = new List<PedHash>();
        List<PedHash> gsfHashList = new List<PedHash>();
        List<Vehicle> ESBVehicles = new List<Vehicle>();
        List<Vehicle> GSFVehicles = new List<Vehicle>();
        int gangSize;

        Vector3[] spawnLocations =
        {
            //new Vector3(98, -1935, 21),
            //new Vector3(319, -2078, 18),
        };

        public SirGerbain_TrafficStop()
        {
            //InitInfo(spawnLocations[random.Next(spawnLocations.Length)]);
            float offsetX = random.Next(175, 300);
            float offsetY = random.Next(175, 300);
            Vector3 playerPos = Game.PlayerPed.Position;
            Vector3 calloutLocation = new Vector3(offsetX, offsetY, 0) + playerPos;
            calloutLocation = World.GetNextPositionOnStreet(calloutLocation);
            InitInfo(calloutLocation);
            
            carHashList.Add(VehicleHash.Baller);
            carHashList.Add(VehicleHash.Buffalo);
            carHashList.Add(VehicleHash.Buccaneer);
            carHashList.Add(VehicleHash.Dominator);
            carHashList.Add(VehicleHash.Futo);
            carHashList.Add(VehicleHash.Gauntlet);
            carHashList.Add(VehicleHash.Picador);
            carHashList.Add(VehicleHash.RancherXL);
            carHashList.Add(VehicleHash.SlamVan);

            esbHashList.Add(PedHash.BallaEast01GMY);
            esbHashList.Add(PedHash.BallaOrig01GMY);
            esbHashList.Add(PedHash.Ballas01GFY);
            esbHashList.Add(PedHash.Ballasog);
            esbHashList.Add(PedHash.BallaSout01GMY);

            gsfHashList.Add(PedHash.Famca01GMY);
            gsfHashList.Add(PedHash.Famdd01);
            gsfHashList.Add(PedHash.Famdnf01GMY);
            gsfHashList.Add(PedHash.Famfor01GMY);
            gsfHashList.Add(PedHash.Families01GFY);

            weaponHashList.Add(WeaponHash.Pistol);
            weaponHashList.Add(WeaponHash.PumpShotgun);
            weaponHashList.Add(WeaponHash.MicroSMG);

            this.ShortName = "Traffic Stop";
            this.CalloutDescription = "Officer needs IMMEDIATE backup at traffic stop.";
            this.ResponseCode = 3;
            this.StartDistance = 200f;
        }

        public async override Task OnAccept()
        {
            InitBlip();
            Notify("Officer needs IMMEDIATE backup at traffic stop");

        }

        public async override void OnStart(Ped player)
        {
            base.OnStart(player);
            await setupCallout();

            EastSideBallaGang[0].Task.ShootAt(GroveStreetFamilies[0]);
            EastSideBallaGang[2].Task.ShootAt(GroveStreetFamilies[2]);
            EastSideBallaGang[3].Task.ShootAt(GroveStreetFamilies[3]);
        }
        public async Task setupCallout()
        {
            gangSize = random.Next(5, 12);
            EastSideBallaGang = new Ped[gangSize];
            GroveStreetFamilies = new Ped[gangSize];

            World.AddRelationshipGroup("ESB");
            World.AddRelationshipGroup("GSF");

            float offsetX_cop = 5.0f * (float)Math.Cos(0 * 120.0f * (Math.PI / 180.0));
            float offsetY_cop = 5.0f * (float)Math.Sin(0 * 120.0f * (Math.PI / 180.0));

            float offsetX_gang = -5.0f * (float)Math.Cos(1 * 120.0f * (Math.PI / 180.0));
            float offsetY_gang = -5.0f * (float)Math.Sin(1 * 120.0f * (Math.PI / 180.0));

            npcPoliceCar = await SpawnVehicle(VehicleHash.Police3, Location+new Vector3(offsetX_cop, offsetY_cop,0), random.Next(360));
            npcPoliceCar.IsSirenActive= true;
            npcPoliceCar.IsSirenSilent= true;
            npcPoliceCar.IsEngineRunning = true;
            npcPoliceCar.AreLightsOn = true;

            npcGangMemberCar = await SpawnVehicle(carHashList[random.Next(carHashList.Count)], Location + new Vector3(offsetX_gang, offsetY_gang, 0), random.Next(360));
            npcGangMemberCar.IsEngineRunning = true;
            npcGangMemberCar.AreLightsOn = true;
            npcGangMemberCar.Mods.PrimaryColor = VehicleColor.MetallicPurple;
            npcGangMemberCar.Mods.SecondaryColor = VehicleColor.MetallicPurple;
            npcGangMemberCar.Mods.PearlescentColor = VehicleColor.MetallicPurple;

            npcOfficer = await SpawnPed(PedHash.Cop01SMY, npcPoliceCar.GetOffsetPosition(new Vector3(0, 5, 0)));
            npcOfficer.Health = 250;
            npcOfficer.Accuracy = 0;
            npcOfficer.Weapons.Give(WeaponHash.Pistol, 1000, true, true);
            npcOfficer.AlwaysKeepTask = true;
            npcOfficer.BlockPermanentEvents = true;
            npcOfficer.Task.GuardCurrentPosition();

            npcGangMember = await SpawnPed(PedHash.BallaEast01GMY, npcGangMemberCar.GetOffsetPosition(new Vector3(0, 5, 0)));
            npcGangMember.Health = 250;
            npcGangMember.Accuracy = 0;
            npcGangMember.Weapons.Give(WeaponHash.Pistol, 1000, true, true);
            npcGangMember.AlwaysKeepTask = true;
            npcGangMember.BlockPermanentEvents = true;
            npcGangMember.RelationshipGroup = GetHashKey("ESB");

            for (int i = 0; i < random.Next(1,5); i++)
            {
                float offsetX = 28.0f * (float)Math.Cos(i * 120.0f * (Math.PI / 180.0));
                float offsetY = 28.0f * (float)Math.Sin(i * 120.0f * (Math.PI / 180.0));
                Vector3 gangVehiclePosition = World.GetNextPositionOnStreet(Location + new Vector3(offsetX, offsetY, 0));

                Vehicle ESBVehicle = await SpawnVehicle(carHashList[random.Next(carHashList.Count)], gangVehiclePosition, random.Next(360));
                        ESBVehicle.IsEngineRunning = true;
                        ESBVehicle.AreLightsOn = true;
                        ESBVehicle.Mods.PrimaryColor = VehicleColor.MetallicPurple;
                        ESBVehicle.Mods.SecondaryColor = VehicleColor.MetallicPurple;
                        ESBVehicle.Mods.PearlescentColor = VehicleColor.MetallicPurple;
                ESBVehicles.Add(ESBVehicle);
            }

            for (int i = 0; i < random.Next(1, 5); i++)
            {
                float offsetX = -28.0f * (float)Math.Cos(i * 120.0f * (Math.PI / 180.0));
                float offsetY = -28.0f * (float)Math.Sin(i * 120.0f * (Math.PI / 180.0));
                Vector3 gangVehiclePosition = World.GetNextPositionOnStreet(Location + new Vector3(offsetX, offsetY, 0));

                Vehicle GSFVehicle = await SpawnVehicle(carHashList[random.Next(carHashList.Count)], gangVehiclePosition, random.Next(360));
                        GSFVehicle.IsEngineRunning = true;
                        GSFVehicle.AreLightsOn = true;
                        GSFVehicle.Mods.PrimaryColor = VehicleColor.MetallicGreen;
                        GSFVehicle.Mods.SecondaryColor = VehicleColor.MetallicGreen;
                        GSFVehicle.Mods.PearlescentColor = VehicleColor.MetallicGreen;
                GSFVehicles.Add(GSFVehicle);
            }
               
            for (int i = 0; i < gangSize; i++)
            {

                float offsetX_esb = 15.0f * (float)Math.Cos(i * 120.0f * (Math.PI / 180.0));
                float offsetY_esb = 15.0f * (float)Math.Sin(i * 120.0f * (Math.PI / 180.0));
                Vector3 gangPedPositionESB = ESBVehicles[random.Next(ESBVehicles.Count)].Position + new Vector3(offsetX_esb, offsetY_esb, 0);

                EastSideBallaGang[i] = await SpawnPed(esbHashList[random.Next(esbHashList.Count)], gangPedPositionESB);
                EastSideBallaGang[i].RelationshipGroup = GetHashKey("ESB");
                EastSideBallaGang[i].Health = 250;
                EastSideBallaGang[i].Accuracy = 0;
                EastSideBallaGang[i].Weapons.Give(weaponHashList[random.Next(weaponHashList.Count)], 1000, true, true);
                EastSideBallaGang[i].Task.GuardCurrentPosition();
                SetPedCombatAttributes(EastSideBallaGang[i].Handle, 45, true);


                float offsetX_gsf = 15.0f * (float)Math.Cos(i * 120.0f * (Math.PI / 180.0));
                float offsetY_gsf = 15.0f * (float)Math.Sin(i * 120.0f * (Math.PI / 180.0));
                Vector3 gangPedPositionGSF = GSFVehicles[random.Next(GSFVehicles.Count)].Position + new Vector3(offsetX_gsf, offsetY_gsf, 0);

                GroveStreetFamilies[i] = await SpawnPed(gsfHashList[random.Next(gsfHashList.Count)], gangPedPositionGSF);
                GroveStreetFamilies[i].RelationshipGroup = GetHashKey("GSF");
                GroveStreetFamilies[i].Health = 250;
                GroveStreetFamilies[i].Accuracy = 0;
                GroveStreetFamilies[i].Weapons.Give(weaponHashList[random.Next(weaponHashList.Count)], 1000, true, true);
                GroveStreetFamilies[i].Task.GuardCurrentPosition();
                SetPedCombatAttributes(GroveStreetFamilies[i].Handle, 45, true);

            }

            setCalloutRelationships();
        }

        public void setCalloutRelationships()
        {
            SetRelationshipBetweenGroups((int)Relationship.Hate, (uint)GetHashKey("ESB"), (uint)GetHashKey("GSF"));
            SetRelationshipBetweenGroups((int)Relationship.Hate, (uint)GetHashKey("GSF"), (uint)GetHashKey("ESB"));
            SetRelationshipBetweenGroups((int)Relationship.Hate, (uint)GetHashKey("PLAYER"), (uint)GetHashKey("ESB"));
            SetRelationshipBetweenGroups((int)Relationship.Hate, (uint)GetHashKey("ESB"), (uint)GetHashKey("PLAYER"));
            SetRelationshipBetweenGroups((int)Relationship.Hate, (uint)GetHashKey("PLAYER"), (uint)GetHashKey("GSF"));
            SetRelationshipBetweenGroups((int)Relationship.Hate, (uint)GetHashKey("GSF"), (uint)GetHashKey("PLAYER"));
            SetRelationshipBetweenGroups((int)Relationship.Companion, (uint)GetHashKey("ESB"), (uint)GetHashKey("ESB"));
            SetRelationshipBetweenGroups((int)Relationship.Companion, (uint)GetHashKey("GSF"), (uint)GetHashKey("GSF"));

        }
        private void Notify(string message)
        {
            ShowNetworkedNotification(message, "CHAR_CALL911", "CHAR_CALL911", "Dispatch", "AIR-1", 15f);
        }
        private void DrawSubtitle(string message, int duration)
        {
            API.BeginTextCommandPrint("STRING");
            API.AddTextComponentSubstringPlayerName(message);
            API.EndTextCommandPrint(duration, false);
        }
    }
}