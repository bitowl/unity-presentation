using UnityEngine;
using System;
using UnityEngine.SceneManagement;


#if UNITY_EDITOR
using UnityEditor;
using Unity.Presentation.EditorOnly;
#endif

namespace Unity.Presentation.Behaviors
{
    /// <summary>
    /// Helper behavior instantiated to slide scenes which forwards Game View events to <see cref="Unity.Presentation.Engine"/>.
    /// </summary>
    [ExecuteInEditMode]
    public class PresentationHelper : MonoBehaviour
    {
        #region Events

        /// <summary>
        /// Previous slide event.
        /// </summary>
        public event EventHandler Previous;

        /// <summary>
        /// Next slide event.
        /// </summary>
        public event EventHandler Next;

        /// <summary>
        /// Frame event.
        /// </summary>
        public event EventHandler Frame;

        public event EventHandler<int> GoToSlide;

        #endregion

        #region Public properties/fields.

        /// <summary>
        /// Previous slide key binding.
        /// </summary>
        [HideInInspector]
        public KeyCode PreviousSlide = KeyCode.LeftArrow;

        /// <summary>
        /// Next slide key binding.
        /// </summary>
        [HideInInspector]
        public KeyCode NextSlide = KeyCode.RightArrow;

        public string[] slideNames;
        public static string BreakScene => "Assets/Scenes/Common/Break.unity";

        #endregion

        #region Private variables

#if UNITY_EDITOR
        private GameView gameView;
#endif

        private bool showSlidesMenu;
        public GUIStyle buttonStyle;
        private bool showCursorCircle;
        private Texture2D cursorTexture;
        private bool showBreakMenu = false;
        private bool isShowingBreak = false;
        private String sceneBeforeBreak;
        #endregion

        #region Unity callbacks

        private void Start()
        {
            buttonStyle = new GUIStyle
            {
                fontSize = 20,
                hover = { textColor = new Color(0.2196078f, 0.4039216f, 0.8392157f) },
                padding = { left = 16, right = 16, top = 8, bottom = 8 },
                border = { bottom = 1 },
                normal = { background = (Texture2D)Resources.Load("gray-square") }
            };
            buttonStyle.hover.background = buttonStyle.normal.background;
            cursorTexture = Resources.Load<Texture2D>("cursor");

        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            gameView = GameView.Instance;
#endif
        }

        private void Update()
        {
            if (Frame != null) Frame(this, EventArgs.Empty);

            keyHandled = false;
        }

        // Getting double EventType.KeyUp events in Standalone Player.
        // This hack is here to make sure that we handle it only once.
        private bool keyHandled = false;

        private void OnGUI()
        {

            if (showSlidesMenu && GoToSlide != null)
            {
                for (int i = 0; i < slideNames.Length; i++)
                {
                    if (GUILayout.Button(i + "  " + slideNames[i], buttonStyle))
                    {
                        showSlidesMenu = false;
                        GoToSlide(this, i);
                    }
                }
            }

            if (showCursorCircle)
            {
                GUI.DrawTexture(new Rect(Input.mousePosition.x - 32, Screen.height - Input.mousePosition.y - 32, 64, 64), cursorTexture);
            }

            if (showBreakMenu)
            {
                if (isShowingBreak)
                {
                    if (GUILayout.Button("Resume", buttonStyle))
                    {
                        showBreakMenu = false;
                        SceneManager.LoadScene(sceneBeforeBreak);
                    }
                }
                else
                {
                    if (GUILayout.Button("Break", buttonStyle))
                    {
                        sceneBeforeBreak = SceneManager.GetActiveScene().path;
                        SceneManager.LoadScene(BreakScene);
                        showBreakMenu = false;
                        isShowingBreak = true;
                    }
                }
                if (GUILayout.Button("Quit", buttonStyle))
                {
#if UNITY_EDITOR
                    Engine.Instance.StopPresentation();
#else
                    Application.Quit();
#endif
                }


            }

            // Key presses

            if (Event.current.type == EventType.KeyUp && !keyHandled)
            {
                keyHandled = true;
                if (Event.current.keyCode == PreviousSlide && Previous != null)
                {
                    Event.current.Use();
                    Previous(this, EventArgs.Empty);
                }
                else if (Event.current.keyCode == NextSlide && Next != null)
                {
                    Event.current.Use();
                    Next(this, EventArgs.Empty);
                }
                else if (Event.current.keyCode == KeyCode.Space && Event.current.shift)
                {
#if UNITY_EDITOR
                    if (Application.isPlaying)
                    {
                        if (gameView.IsMaximized || gameView.IsFullscreen)
                            gameView.SetNormal();
                        else
                        {
                            if (Event.current.control || Event.current.command)
                                gameView.SetFullscreen();
                            else
                                gameView.SetMaximized();
                        }
                    }
                    else
                    {
                        if (gameView.IsFullscreen)
                            gameView.SetNormal();
                        else if (Event.current.control || Event.current.command)
                            gameView.SetFullscreen();
                    }
#endif
                }
                else if (Event.current.keyCode == KeyCode.Escape)
                {
                    showBreakMenu = !showBreakMenu;
                }
                else if (Event.current.keyCode == KeyCode.Q)
                {
                    showSlidesMenu = !showSlidesMenu;
                }
                else if (Event.current.keyCode == KeyCode.C)
                {
                    showCursorCircle = !showCursorCircle;
                }
            }
        }

        private void OnDestroy()
        {
            Previous = null;
            Next = null;
            Frame = null;
        }

        #endregion
    }
}