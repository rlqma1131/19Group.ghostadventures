using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_SewerMusicPuzzle : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private float warningTime = 1f;
    [SerializeField] private float musicSegmentDuration = 4f;
    [SerializeField] private float musicPauseDuration = 1.5f;

    private Coroutine musicRoutine;
    private bool isPlaying = false;
    private float songStartTime;
    private bool movedDuringPlay = false;

    public void StartPuzzle()
    {
        if (musicRoutine == null)
        {
            musicRoutine = StartCoroutine(MusicLoop());
        }
    }

    public void StopPuzzle()
    {
        if (musicRoutine != null)
        {
            StopCoroutine(musicRoutine);
            musicRoutine = null;
        }

        musicSource.Stop();
        isPlaying = false;
        Debug.Log("퍼즐 중지");
    }

    private IEnumerator MusicLoop()
    {
        while (!Ch2_SewerPuzzleManager.Instance.IsPuzzleSolved())
        {
            yield return new WaitForSeconds(Random.Range(2f, 10f)); // 재생 간격

            // 음악 재생 준비
            float maxStart = Mathf.Max(1f, musicSource.clip.length - musicSegmentDuration);
            musicSource.time = Random.Range(0f, maxStart);
            musicSource.Play();

            isPlaying = true;
            songStartTime = Time.time;
            movedDuringPlay = false;

            yield return new WaitForSeconds(musicSegmentDuration);

            musicSource.Stop();
            isPlaying = false;

            if (movedDuringPlay)
            {
                TriggerPunishment();
            }

            yield return new WaitForSeconds(musicPauseDuration);
        }
    }

    private void Update()
    {
        if (!isPlaying) return;

        float elapsed = Time.time - songStartTime;

        // 경고 시간 이전 - 감시 X
        if (elapsed <= warningTime)
        {
            return;
        }

        // 경고 시간 이후 - 재생 끝나는 시점까지만 감시
        if (elapsed > warningTime && elapsed <= musicSegmentDuration)
        {
            if (IsPlayerMoving())
            {
                movedDuringPlay = true;
            }
        }
    }

    private bool IsPlayerMoving()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        return Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;
    }

    private void TriggerPunishment()
    {
        Debug.Log("음악 재생 중 이동 감지 - 사신 트리거 ( 사운드 트리거 삽입 자리 )");
    }
}
