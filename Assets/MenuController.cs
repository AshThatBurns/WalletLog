using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{

    [System.Serializable]
    public class Menu
    {
        [HideInInspector] public Vector2 closePos = new Vector2(-439.6f, 0f);
        [HideInInspector] public Vector2 openPos = new Vector2(-43.6f, 0f);
        [HideInInspector] public Vector2 dest = Vector2.zero;
        [HideInInspector] public float speed = 10f;

        [HideInInspector] public enum MoveState { None, open, moving, closed };
        [HideInInspector] public MoveState moveState = MoveState.closed;
        [HideInInspector] public MoveState destState = MoveState.None;

        [HideInInspector] public RectTransform panel;
    }
    [SerializeField] public Menu menu;

    public int defaultPageNumber;
    public GameObject[] pages;
    public GameObject loadMessagesPanel;

    public static MenuController instance;

    private void OnEnable()
    {
        menu = new Menu();
        menu.panel = GetComponent<RectTransform>();
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        loadMessagesPanel.SetActive(true);
        goToDefaultPage();
    }

    // Update is called once per frame
    void Update()
    {
        // Move Debug Panel
        if (menu.moveState == Menu.MoveState.moving)
        {
            menu.panel.anchoredPosition = Vector2.Lerp(menu.panel.anchoredPosition, menu.dest, menu.speed * Time.deltaTime);

            // if dest is reached
            if (menu.panel.anchoredPosition.y - menu.dest.y < 0.1f)
            {
                menu.moveState = menu.destState;
                menu.panel.anchoredPosition = menu.dest;
            }
        }
    }

    public void OpenPageNumber(int pageNumber)
    {
        hideAllPages();

        closeMenu();
        pages[pageNumber].SetActive(true);
    }

    public void OnOpenButtonPress()
    {
        if (menu.moveState == Menu.MoveState.moving) return;

        // Set dest pos
        menu.destState = menu.moveState == Menu.MoveState.closed ? Menu.MoveState.open : Menu.MoveState.closed;
        menu.dest = menu.destState == Menu.MoveState.closed ? menu.closePos : menu.openPos;
        menu.moveState = Menu.MoveState.moving;
    }

    void closeMenu()
    {
        menu.destState = Menu.MoveState.closed;
        menu.dest = menu.destState == Menu.MoveState.closed ? menu.closePos : menu.openPos;
        menu.moveState = Menu.MoveState.moving;
    }

    void goToDefaultPage()
    {
        hideAllPages();
        pages[defaultPageNumber].SetActive(true);
    }

    void hideAllPages()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
        }
    }
}
