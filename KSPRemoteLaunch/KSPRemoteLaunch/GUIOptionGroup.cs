using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KSPRemoteLaunch
{
    class GUIOptionGroup
    {
        private List<GUIOptionButton> buttons;
        private GUIOptionButton activeButton;
        private Vector2 scrollPos = new Vector2(0, 0);
        public delegate void onSelected(bool enabled);
        public delegate void onDelete();
        public delegate void onUpdate();

        public GUIOptionGroup()
        {
            buttons = new List<GUIOptionButton>();
        }

        public void setStyle(GUIStyle style)
        {
            foreach (GUIOptionButton btn in buttons)
            {
                btn.setStyle(style);
            }
        }

        public void setActiveButtonText(string text)
        {
            activeButton.Text = text;
        }

        public void updateActiveButton()
        {
            activeButton.update();
        }

        public void deleteActiveButton()
        {
            activeButton.delete();
            buttons.Remove(activeButton);
        }

        public void addToggleButton(string text, onSelected action, onDelete delAction, onUpdate updateAction)
        {
            buttons.Add(new GUIOptionButton(text, action, delAction,updateAction));

        }

        public void setActiveToggleButton(GUIOptionButton toggleButton)
        {
            changeActiveToggleButton(toggleButton);
        }

        public void addActiveToggleButton(string text, onSelected action, onDelete delAction, onUpdate updateAction)
        {
            GUIOptionButton toggleButton = new GUIOptionButton(text, action, delAction,updateAction);
            buttons.Add(toggleButton);
            changeActiveToggleButton(toggleButton);
        }

        //add GUI params?
        public void checkSelected()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUI.skin.scrollView, GUILayout.Width(210.0f));
            foreach (GUIOptionButton btn in buttons)
            {
                if (btn.CheckPressed() && btn != activeButton)
                {
                    Debug.Log("Previous Active Button: " + activeButton);
                    activeButton.dissable();
                    btn.enable();
                    activeButton = btn;
                    Debug.Log("Current Active Button: " + activeButton);
                }
            }
            GUILayout.EndScrollView();
        }

        private void changeActiveToggleButton(GUIOptionButton toggleButton)
        {
            if (activeButton != null)
            {
                activeButton.dissable();
            }
            toggleButton.enable();
            activeButton = toggleButton;
        }


        internal class GUIOptionButton
        {
            private GUIStyle buttonStyle;
            private string GUIText;
            private bool enabled = false;
            private onSelected selectedAction;
            private onDelete deleteAction;
            private onDelete delAction;
            private onUpdate updateAction;

            public string Text
            {
                get { return GUIText; }
                set { GUIText = value; }
            }

            //not used any more
            public GUIOptionButton(string text, onSelected action, onDelete delAction)
            {
                selectedAction = action;
                deleteAction = delAction;
                GUIText = text;

            }

            public GUIOptionButton(string text, onSelected action, onDelete delAction, onUpdate updateAction)
            {
                // TODO: Complete member initialization
                this.GUIText = text;
                this.selectedAction = action;
                this.delAction = delAction;
                this.updateAction = updateAction;
            }

            public void setStyle(GUIStyle style)
            {
                buttonStyle = style;
            }

            
            //Button must be enabled to be pressed
            public bool CheckPressed()
            {
                if (GUILayout.Toggle(enabled, GUIText, buttonStyle, GUILayout.Width(174.0f)))
                {
                    return true;
                }
                return false;
            }

            public void dissable()
            {
                enabled = false;
            }
            public void enable()
            {
                enabled = true;
                selectedAction(enabled);
            }

            public void delete()
            {
                deleteAction();
            }

            public override string ToString()
            {
                return GUIText;
            }

            internal void update()
            {
                updateAction();
            }

            
        }
    }
}
