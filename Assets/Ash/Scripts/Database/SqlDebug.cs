using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SqlDebug : MonoBehaviour
{
    [System.Serializable] public class SqlDebugPanel
    {
        [HideInInspector] public Vector2 closePos = new Vector2(0, 758);
        [HideInInspector] public Vector2 openPos = new Vector2(0, 220);
        [HideInInspector] public Vector2 dest = Vector2.zero;
        [HideInInspector] public float speed = 10f;

        [HideInInspector] public enum MoveState { None, open, moving, closed };
        [HideInInspector] public MoveState moveState = MoveState.closed;
        [HideInInspector] public MoveState destState = MoveState.None;

        [HideInInspector] public RectTransform panel;
        public InputField inputField;
    }
    [SerializeField] public SqlDebugPanel sqlDebugPanel;

    public static SqlDebug instance;

    private void OnEnable()
    {
        sqlDebugPanel = new SqlDebugPanel();
        sqlDebugPanel.panel = GetComponent<RectTransform>();
    }

    private void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        // Move Debug Panel
        if (sqlDebugPanel.moveState == SqlDebugPanel.MoveState.moving)
        {
            sqlDebugPanel.panel.anchoredPosition = Vector2.Lerp(sqlDebugPanel.panel.anchoredPosition, sqlDebugPanel.dest, sqlDebugPanel.speed * Time.deltaTime);
            
            // if dest is reached
            if (sqlDebugPanel.panel.anchoredPosition.y - sqlDebugPanel.dest.y < 0.1f)
            {
                sqlDebugPanel.moveState = sqlDebugPanel.destState;
                sqlDebugPanel.panel.anchoredPosition = sqlDebugPanel.dest;
            }
        }
    }

    public void OnOpenButtonPress()
    {
        if (sqlDebugPanel.moveState == SqlDebugPanel.MoveState.moving) return;

        // Set dest pos
        sqlDebugPanel.destState = sqlDebugPanel.moveState == SqlDebugPanel.MoveState.closed ? SqlDebugPanel.MoveState.open : SqlDebugPanel.MoveState.closed;
        sqlDebugPanel.dest = sqlDebugPanel.destState == SqlDebugPanel.MoveState.closed ? sqlDebugPanel.closePos : sqlDebugPanel.openPos;
        sqlDebugPanel.moveState = SqlDebugPanel.MoveState.moving;
    }    

}
