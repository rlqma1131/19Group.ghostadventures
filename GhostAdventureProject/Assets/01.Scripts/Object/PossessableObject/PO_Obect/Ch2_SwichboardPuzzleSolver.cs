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
                DFS(buttons, System.Array.IndexOf(buttons, button), visited);
                if (AllVisited(visited) && EndVisited(buttons, visited))
                    return true;
            }
        }
        return false;
    }

    static bool IsConnectedToStart(Ch2_SwitchboardButton button)
    {
        // 시작 조건: id==0이고, left가 열려 있으면 연결 시작
        return button.id == 0 && button.connection.left;
    }

    static bool AllVisited(bool[] visited)
    {
        foreach (var v in visited)
            if (!v) return false;
        return true;
    }

    static bool EndVisited(Ch2_SwitchboardButton[] buttons, bool[] visited)
    {
        for (int i = 0; i < buttons.Length; i++)
            if (buttons[i].id == 5 && visited[i])
                return true;
        return false;
    }

    static void DFS(Ch2_SwitchboardButton[] buttons, int index, bool[] visited)
    {
        if (visited[index]) return;
        visited[index] = true;
        for (int i = 0; i < buttons.Length; i++)
        {
            if (visited[i]) continue;
            if (IsConnected(buttons[index], buttons[i]))
                DFS(buttons, i, visited);
        }
    }

    static bool IsConnected(Ch2_SwitchboardButton a, Ch2_SwitchboardButton b)
    {
        if (a.connection.right && b.connection.left) return true;
        if (a.connection.left && b.connection.right) return true;
        if (a.connection.top && b.connection.bottom) return true;
        if (a.connection.bottom && b.connection.top) return true;
        return false;
    }
}
