namespace TaoTie
{
    public static class AttackHelper
    {
        /// <summary>
        /// 检查是否是敌人
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool CheckIsEnemy(Entity actor, Entity other)
        {
            if (actor.Type == EntityType.Avatar && other.Type == EntityType.Monster)
                return true;
            if (actor.Type == EntityType.Monster && other.Type == EntityType.Avatar)
                return true;
            
            //todo:
            return false;
        }
        /// <summary>
        /// 检查是否是队友
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool CheckIsCamp(Entity actor, Entity other)
        {
            //todo:
            return actor.Type == other.Type;
        }
        
    }
}