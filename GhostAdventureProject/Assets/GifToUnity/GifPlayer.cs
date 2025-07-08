using GifImporter;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GifImporter
{
    [ExecuteAlways]
    public class GifPlayer : MonoBehaviour
    {
        public Gif Gif;

        private int _index;
        private float _flip;
        private Gif _setGif;

        private bool _isPaused = false; // 일시정지 상태 플래그
        private bool _hasFinishedPlaying = false; // 재생 완료 상태 플래그

        private void OnEnable()
        {

            if (Gif == null) return;
            var frames = Gif.Frames;
            if (frames == null || frames.Count == 0) return;

            if (_index > frames.Count - 1)
            {
                _index = _index % frames.Count;
            }

            var frame = frames[_index];
            Apply(frame);
            _hasFinishedPlaying = false; // 활성화 시 재생 완료 상태 초기화
        }

     


        private void Update()
        {
            if (_isPaused || _hasFinishedPlaying) return; // 일시정지 중이거나 재생이 끝났으면 아무것도 하지 않음

            if (Gif == null) return;
            var frames = Gif.Frames;
            if (frames == null || frames.Count == 0) return;

            int index = _index;

            if (Application.isPlaying && _flip < Time.unscaledTime)
            {
                index++;
            }

            if (index > frames.Count - 1)
            {
                
                _hasFinishedPlaying = true; // 재생 완료 상태로 설정
                index = frames.Count - 1; // 마지막 프레임으로 고정
            }

            if (index != _index || _setGif != Gif)
            {
                _index = index;
                var frame = frames[_index];
                Apply(frame);
            }
        }

        private void Apply(GifFrame frame)
        {
            Image image = null;
            if (TryGetComponent<SpriteRenderer>(out var spriteRenderer) || TryGetComponent(out image))
            {
                _flip = Time.unscaledTime + frame.DelayInMs * 0.001f;

                if (spriteRenderer != null) spriteRenderer.sprite = frame.Sprite;
                else if (image != null) image.sprite = frame.Sprite;

                _setGif = Gif;
            }
        }

        // ✅ 일시정지 함수
        public void Pause()
        {
            _isPaused = true;
            Debug.Log("GIF playback paused.");
        }

        // 다시 재생 함수
        public void Resume()
        {
            _isPaused = false;
         
            
            Debug.Log("Resuming GIF playback...");
            if (_hasFinishedPlaying && Gif != null && Gif.Frames.Count > 0)
            {
                _index = 0; // 처음부터 다시 시작하려면 인덱스를 0으로 리셋
                _hasFinishedPlaying = false; // 재생 완료 상태 해제
                _flip = Time.unscaledTime + Gif.Frames[_index].DelayInMs * 0.001f;
                Apply(Gif.Frames[_index]); // 첫 프레임 적용
            }
            else if (!_hasFinishedPlaying && Gif != null && Gif.Frames.Count > 0)
            {
                _flip = Time.unscaledTime + Gif.Frames[_index].DelayInMs * 0.001f;
            }
        }

        //  처음부터 다시 재생
        public void Restart()
        {
            _index = 0;
            _hasFinishedPlaying = false;
            _isPaused = false;
            if (Gif != null && Gif.Frames.Count > 0)
            {
                _flip = Time.unscaledTime + Gif.Frames[_index].DelayInMs * 0.001f;
                Apply(Gif.Frames[_index]);
            }
        }
    }
}