using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Ch2_SwichboardPuzzleSolver
{
    public static bool IsPathConnected(Ch2_SwitchboardButton[] buttons)
    {
        foreach (var button in buttons)
        {
            if (IsConnectedToStart(button))
            {
                bool[] visited = new bool[buttons.Length];
                int index = System.Array.IndexOf(buttons, button);

                if (DFS(buttons, index, visited))
                    return true;
            }
        }

        return false;
    }

    static bool DFS(Ch2_SwitchboardButton[] buttons, int index, bool[] visited)
    {
        if (IsConnectedToEnd(buttons[index]))
        {
            Debug.Log("!!! End connected !!!");
            return true;
        }

        visited[index] = true;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (visited[i]) continue;

            if (IsConnected(buttons[index], buttons[i]))
            {
                Debug.Log($"--> {buttons[index].id} to {buttons[i].id} 연결!");
                if (DFS(buttons, i, visited))
                    return true;
            }
        }

        return false;
    }
    static bool IsConnected(Ch2_SwitchboardButton a, Ch2_SwitchboardButton b)
    {
        if (a.connection.right && b.connection.left) return true;
        if (a.connection.left && b.connection.right) return true;
        if (a.connection.top && b.connection.bottom) return true;
        if (a.connection.bottom && b.connection.top) return true;
        return false;
    }

    static bool IsConnectedToStart(Ch2_SwitchboardButton button)
    {
        // 시작 조건: id==0이고, left가 열려 있으면 연결 시작
        return button.id == 0 && button.connection.left;
    }

    static bool IsConnectedToEnd(Ch2_SwitchboardButton button)
    {
        // 끝 조건: id==5이고, right가 열려 있으면 정답!
        return button.id == 5 && button.connection.right;
    }
}
