using System.Collections;
using TMPro;
using UnityEngine;

public class MovesSystem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float moveTime;

    [Header("References")]
    [SerializeField] GameManager gameManager;
    [SerializeField] HUDManager hudManager;
    [SerializeField] CameraController cameraController;

    public bool IsPlayerTurn { get; private set; }

    PlayerManager playerManager;
    BotManager botManager;
    Coroutine timerCO;
    Director director;
    Helper helper;
    float currentTime;
    bool isGameFinished;

    void Start()
    {
        director = Director.Instance;
        helper = Helper.Instance;
        playerManager = gameManager.PlayerManager;
        IsPlayerTurn = true;

        hudManager.UpdateSideTurnText(false);
    }

    // �������� ���
    public void StartMove(float delay = 0)
    {
        helper.PerformWithDelay(delay, () =>
        {
            if (isGameFinished) return;

            // ��� �����
            if (!IsPlayerTurn)
            {
                bool botFound = false;

                // ���� ���� ������� ����� ������
                foreach (var bot in gameManager.BotManagers)
                {
                    if (bot == null || bot.MoveWasMade) continue;

                    bot.MoveWasMade = true;
                    botFound = bot.TurnState(true);

                    // ���� ��� ����� ������
                    if (botFound)
                    {
                        botManager = bot;
                        hudManager.UpdateSideTurnText(true);
                        cameraController.ChangeTarget(bot.transform);
                    }

                    break;
                }

                // ���� ��� ���� ������� ���� ���
                if (!botFound)
                {
                    IsPlayerTurn = true;
                    StartMove();

                    return;
                }
            }
            // ��� ������
            else
            {
                hudManager.UpdateSideTurnText(true, true);
                playerManager.TurnState(true);
                cameraController.ChangeTarget(playerManager.transform);
            }

            StartTimer();
        });
    }

    // ����������� ���
    public void StopMove()
    {
        StopTimer();

        if (IsPlayerTurn)
        {
            playerManager.TurnState(false);

            IsPlayerTurn = false;
            ClearBots();
        }
        else if (botManager != null) botManager.TurnState(false);

        // �������� ��� ������
        StartMove(director.GameSettings.TurnSwitchDelay);
    }

    // �������� ������������� ����
    public void FinishGame()
    {
        isGameFinished = true;

        StopTimer();
        ClearBots();
        hudManager.UpdateTimer(0);
        hudManager.UpdateSideTurnText(false);

        if (!IsPlayerTurn && botManager != null) botManager.TurnState(false);
        else playerManager.TurnState(false);

        IsPlayerTurn = true;
    }

    // ������������� ����
    public void RestartGame(float delay = 0)
    {
        isGameFinished = false;

        StartMove(delay);
    }

    // ���������� ���� ���� �����
    void ClearBots()
    {
        foreach (var bot in gameManager.BotManagers)
            bot.MoveWasMade = false;
    }

    // ��������� ������ ����
    void StartTimer()
    {
        StopTimer();

        timerCO = StartCoroutine(Timer());
    }

    // ������������� ������
    public void StopTimer()
    {
        if (timerCO != null)
            StopCoroutine(timerCO);
    }

    // ������ ����
    IEnumerator Timer()
    {
        float startValue = moveTime + 0.49f; // TEMP
        float elapsedTime = 0;

        while (elapsedTime < moveTime)
        {
            elapsedTime += Time.deltaTime;
            currentTime = Mathf.Lerp(startValue, 0, elapsedTime / moveTime);

            hudManager.UpdateTimer(currentTime);

            yield return null;
        }

        currentTime = 0;

        StopMove();
        hudManager.UpdateTimer(0);
    }
}
