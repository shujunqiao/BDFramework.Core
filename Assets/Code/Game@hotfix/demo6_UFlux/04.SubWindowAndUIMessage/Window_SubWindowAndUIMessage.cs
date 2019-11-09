using BDFramework.UFlux.item;
using BDFramework.UFlux.Reducer;
using BDFramework.UFlux.Store;
using BDFramework.UFlux.View.Props;
using Code.Game.demo6_UFlux;
using ILRuntime.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace BDFramework.UFlux.UFluxTest004
{
    public enum WinMsg
    {
        test001 = 0,
    }

    public enum SubWindow
    {
        test001
    }

    [UI((int) UFluxWindowEnum.Test004, "Windows/UFlux/demo004/Window_SubWindowAndUIMessage")]
    public class Window_Demo004 : AWindow
    {
        public Window_Demo004(string path) : base(path)
        {
        }

        public Window_Demo004(Transform transform) : base(transform)
        {
        }

        [TransformPath("btn_OpenSubWin")]
        private Button btn_OpenSubWin;

        [TransformPath("btn_CloseSubWin")]
        private Button btn_CloseSubWin;

        [TransformPath("btn_SendMessage ")]
        private Button btn_SndMessage;

        [TransformPath("Content")]
        private Text Content;

        public override void Init()
        {
            base.Init();

            //注册子窗口
            var trans = this.Transform.Find("SubWindow");
            RegisterSubWindow(SubWindow.test001, new SubWindow_Demo004(trans));

            btn_OpenSubWin.onClick.AddListener(() =>
            {
                GetSubWindow<SubWindow_Demo004>(SubWindow.test001).Open();
            });

            btn_CloseSubWin.onClick.AddListener(() =>
            {
                GetSubWindow<SubWindow_Demo004>(SubWindow.test001).Close();
            });

            btn_SndMessage.onClick.AddListener(() =>
            {
                var msg = new UIMessageData(WinMsg.test001, "我是一个测试消息");

                UIManager.Inst.SendMessage(UFluxWindowEnum.Test004, msg);
                
            });
        }


        [UIMessage((int) WinMsg.test001)]
        private void TestMessage(UIMessageData msg)
        {
            Content.text = "父窗口收到消息:" + msg.GetData<string>();
        }
    }
}