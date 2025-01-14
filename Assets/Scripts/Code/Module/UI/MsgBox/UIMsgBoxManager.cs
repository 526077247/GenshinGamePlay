using System.Collections.Generic;

namespace TaoTie
{
    public class UIMsgBoxManager: IManager
    {
        
        public static UIMsgBoxManager Instance;

        private int index;
        private UIBaseView currentMsgBox;
        private LinkedList<MsgBoxInfoNode> stack;
        public void Init()
        {
            Instance = this;
            stack = new LinkedList<MsgBoxInfoNode>();
        }

        public void Destroy()
        {
            Instance = null;
            foreach (var item in stack)
            {
                item.Dispose();
            }

            stack = null;
        }
        

        public async ETTask OpenMsgBox<T>(string path, MsgBoxPara para,UILayerNames layer = UILayerNames.TipLayer)
            where T : UIBaseView, IOnCreate, IOnEnable<MsgBoxPara>
        {
            using (await CoroutineLockManager.Instance.Wait(CoroutineLockType.UIMsgBox, 0))
            {
                if (currentMsgBox != null)
                {
                    MsgBoxInfoNode node = MsgBoxInfoNode.Create();
                    node.Type = TypeInfo<T>.Type;
                    node.Para = para;
                    node.Path = path;
                    node.Layer = layer;
                    stack.AddLast(node);
                    return;
                }

                currentMsgBox = await UIManager.Instance.OpenWindow<T, MsgBoxPara>(path, para, layer);
            }
        }

        public async ETTask CloseMsgBox(UIBaseView msgBox)
        {
            if (currentMsgBox == null || msgBox != currentMsgBox)
            {
                Log.Error("currentMsgBox == null || msgBox != currentMsgBox");
                await msgBox.CloseSelf();
                return;
            }
            await currentMsgBox.CloseSelf();
            if (stack.Count > 0)
            {
                var node = stack.First.Value;
                stack.RemoveFirst();
                currentMsgBox = await UIManager.Instance.OpenWindow(node.Type.FullName, node.Path, node.Para,node.Layer);
                node.Dispose();
            }
            else
            {
                currentMsgBox = null;
            }
        }
    }
}