using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActorType
{
    Player,
    Oni,
    Taka_Nyudo,
    Okuri_Inu,
    Spike_Trap,
    Crush_Trap,
    Pit_Trap,
    Dart_Trap,
    Dart_Projectile,
    Tripwire,
    Lantern_Trap,
    Chalk_Pickup,
    Ofuda_Pickup,
    Mirror_Pickup,
    Compass_Pickup,
    Pressure_Plate,
    Lever,
    Ladder,
    Chalk_Mark,
    Ofuda_Projectile,
    Player_Footprint,
    Oni_Footprint,
    Taka_Nyudo_Footprint,
    Okuri_Inu_Footprint,
    Null
}

public class Actors : MonoBehaviour {

    public static Dictionary<ActorType, GameObject> Prefabs;
    private static bool initialized = false;

    // get field call init?

    public static void InitPrefabs()
    {
        if(!initialized)
        {
            //Prefabs.Add(ActorType.Player, Resources.Load("Prefabs/ChalkMark") as GameObject);
            //Prefabs.Add(ActorType.Oni, Resources.Load("Prefabs/ChalkMark") as GameObject);
            //Prefabs.Add(ActorType.Taka_Nyudo, Resources.Load("Prefabs/ChalkMark") as GameObject);
            //Prefabs.Add(ActorType.Okuri_Inu, Resources.Load("Prefabs/ChalkMark") as GameObject);
            Prefabs.Add(ActorType.Spike_Trap, Resources.Load("Prefabs/Traps/SpikeTrapPrefab") as GameObject);
            Prefabs.Add(ActorType.Crush_Trap, Resources.Load("Prefabs/Traps/CrushingTrapPrefab") as GameObject);
            //Prefabs.Add(ActorType.Pit_Trap, Resources.Load("Prefabs/ChalkMark") as GameObject);
            //Prefabs.Add(ActorType.Dart_Trap, Resources.Load("Prefabs/ChalkMark") as GameObject);
           // Prefabs.Add(ActorType.Dart_Projectile, Resources.Load("Prefabs/ChalkMark") as GameObject);
            //Prefabs.Add(ActorType.Tripwire, Resources.Load("Prefabs/ChalkMark") as GameObject);
            //Prefabs.Add(ActorType.Lantern_Trap, Resources.Load("Prefabs/ChalkMark") as GameObject);
            Prefabs.Add(ActorType.Chalk_Pickup, Resources.Load("Prefabs/Pickups/ChalkPickup") as GameObject);
            Prefabs.Add(ActorType.Ofuda_Pickup, Resources.Load("Prefabs/Pickups/OfudaPickup") as GameObject);
            Prefabs.Add(ActorType.Mirror_Pickup, Resources.Load("Prefabs/Pickups/MirrorPickup") as GameObject);
            Prefabs.Add(ActorType.Compass_Pickup, Resources.Load("Prefabs/Pickups/CompassPickup") as GameObject);
            //Prefabs.Add(ActorType.Pressure_Plate, Resources.Load("Prefabs/ChalkMark") as GameObject);
            //Prefabs.Add(ActorType.Lever, Resources.Load("Prefabs/ChalkMark") as GameObject);
            //Prefabs.Add(ActorType.Chalk_Mark, Resources.Load("Prefabs/ChalkMark") as GameObject);
            //Prefabs.Add(ActorType.Ofuda_Projectile, Resources.Load("Prefabs/ChalkMark") as GameObject);
            //Prefabs.Add(ActorType.Player_Footprint, Resources.Load("Prefabs/ChalkMark") as GameObject);
            //Prefabs.Add(ActorType.Oni_Footprint, Resources.Load("Prefabs/ChalkMark") as GameObject);
            //Prefabs.Add(ActorType.Taka_Nyudo_Footprint, Resources.Load("Prefabs/ChalkMark") as GameObject);
            //Prefabs.Add(ActorType.Okuri_Inu_Footprint, Resources.Load("Prefabs/ChalkMark") as GameObject);
        }
    }
}
