﻿namespace TaoTie
{
    public enum EntityType: byte
    {
        Avatar = 1,
        Monster = 2,
        Gadget = 3,
        SceneGroup = 4,
        Zone = 5,
        Equip = 6,
        Effect = 7,
        
        MAX,
        ALL,
    }

    public enum ActorType: byte
    {
        Avatar = EntityType.Avatar,
        Monster = EntityType.Monster,
        Gadget = EntityType.Gadget,
    }
}