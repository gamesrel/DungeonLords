//defines where is the wall facing.
public enum WALL {
    UP = 0,
    DOWN = 1,
    RIGHT = 2,
    LEFT = 3
}


// Represents the owner of a selected tile.
public enum OWNER {
    IMPENETRABLE = -4,
    INFINITEGOLD = -3,
    GOLD = -2,
    NONE = -1, // Black in the fog of war.
    UNOWNED = 0,//sand
    FIRST = 1,
    SECOND = 2,
    THIRD = 3,
    FOURTH = 4,
    FIFTH = 5,
    SIXTH = 6,
    SEVENTH = 7,
    EIGHT = 8
}

//type of player
public enum PLAYER {
    HERO = 0,
    DEVILGOD = 1
}

public enum TASK {
    DIG = 0,
    CONVERTFLOORTILE = 1,//converts a tile to the player's owner
    CONVERTWALLTILE = 2
}

public enum PATH {
    UNREACHEABLE = 999999
}