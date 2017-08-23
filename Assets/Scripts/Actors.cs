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

    public static Dictionary<ActorType, GameObject> prefabs;
    public static Dictionary<ActorType, GameObject> Prefabs
    {
        get
        {
            InitPrefabs();
            return prefabs;
        }
    }

    private static bool initialized = false;

    // get field call init?

    private static void InitPrefabs()
    {
        if(!initialized)
        {
            prefabs = new Dictionary<ActorType, GameObject>();
            initialized = true;
            prefabs.Add(ActorType.Player, Resources.Load("Prefabs/Player/FPS_Player") as GameObject);
            prefabs.Add(ActorType.Oni, Resources.Load("Prefabs/Enemy/Oni") as GameObject);
            prefabs.Add(ActorType.Taka_Nyudo, Resources.Load("Prefabs/Enemy/TakaNyudo") as GameObject);
            prefabs.Add(ActorType.Okuri_Inu, Resources.Load("Prefabs/Enemy/OkuriInu") as GameObject);
            prefabs.Add(ActorType.Spike_Trap, Resources.Load("Prefabs/Traps/SpikeTrapPrefab") as GameObject);
            prefabs.Add(ActorType.Crush_Trap, Resources.Load("Prefabs/Traps/CrushingTrapPrefab") as GameObject);
            //prefabs.Add(ActorType.Pit_Trap, Resources.Load("Prefabs/ChalkMark") as GameObject);
            //prefabs.Add(ActorType.Dart_Trap, Resources.Load("Prefabs/ChalkMark") as GameObject);
            //prefabs.Add(ActorType.Dart_Projectile, Resources.Load("Prefabs/ChalkMark") as GameObject);
            //prefabs.Add(ActorType.Tripwire, Resources.Load("Prefabs/ChalkMark") as GameObject);
            //prefabs.Add(ActorType.Lantern_Trap, Resources.Load("Prefabs/ChalkMark") as GameObject);
            prefabs.Add(ActorType.Chalk_Pickup, Resources.Load("Prefabs/Pickups/ChalkPickup") as GameObject);
            prefabs.Add(ActorType.Ofuda_Pickup, Resources.Load("Prefabs/Pickups/OfudaPickup") as GameObject);
            prefabs.Add(ActorType.Mirror_Pickup, Resources.Load("Prefabs/Pickups/MirrorPickup") as GameObject);
            prefabs.Add(ActorType.Compass_Pickup, Resources.Load("Prefabs/Pickups/CompassPickup") as GameObject);
            //prefabs.Add(ActorType.Pressure_Plate, Resources.Load("Prefabs/ChalkMark") as GameObject);
            //prefabs.Add(ActorType.Lever, Resources.Load("Prefabs/ChalkMark") as GameObject);
            prefabs.Add(ActorType.Chalk_Mark, Resources.Load("Prefabs/Player/ChalkMark") as GameObject);
            prefabs.Add(ActorType.Ofuda_Projectile, Resources.Load("Prefabs/Player/OfudaProjectile") as GameObject);
            prefabs.Add(ActorType.Player_Footprint, Resources.Load("Prefabs/Player/Footprint") as GameObject);
            prefabs.Add(ActorType.Oni_Footprint, Resources.Load("Prefabs/Enemy/OniFootprint") as GameObject);
            //prefabs.Add(ActorType.Taka_Nyudo_Footprint, Resources.Load("Prefabs/ChalkMark") as GameObject);
            prefabs.Add(ActorType.Okuri_Inu_Footprint, Resources.Load("Prefabs/Enemy/InuFootprint") as GameObject);
            prefabs.Add(ActorType.Ladder, Resources.Load("Prefabs/Level/Ladder") as GameObject);
        }
    }
}
