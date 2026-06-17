using UnityEngine;
using System.Collections;
using System;

public class PromotionVFX : MonoBehaviour
{
    private Coroutine spinCoroutine;

    // Memutar bidak pion yang akan promosi dengan sangat cepat dan membuatnya melayang
    public void PlayPawnSpin()
    {
        spinCoroutine = StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;
        while (true)
        {
            elapsed += Time.deltaTime;
            // Berputar dengan sangat cepat (1000 derajat per detik)
            transform.Rotate(Vector3.up, 1000f * Time.deltaTime, Space.World);
            // Melayang naik-turun menggunakan kurva Sinus
            transform.position = startPos + new Vector3(0, Mathf.Sin(elapsed * 5f) * 0.4f, 0);
            yield return null;
        }
    }

    // Memunculkan bidak baru (Queen/Knight/dsb) dengan efek pop-up yang memantul elastis
    public void PlaySpawnPop(Action onComplete)
    {
        if (spinCoroutine != null) StopCoroutine(spinCoroutine);
        StartCoroutine(PopRoutine(onComplete));
    }

    private IEnumerator PopRoutine(Action onComplete)
    {
        if (MusicManager.Instance != null) MusicManager.Instance.PlayPromotion();

        Vector3 originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        
        float elapsed = 0f;
        float duration = 0.6f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Efek elastis Bouncy (Membesar lebih dari ukuran asli, lalu kembali normal)
            float scaleT = Mathf.Sin(t * Mathf.PI * 0.5f); 
            scaleT += Mathf.Sin(t * Mathf.PI) * (1f - t) * 0.4f; 

            transform.localScale = originalScale * scaleT;
            yield return null;
        }
        
        transform.localScale = originalScale;
        
        // Jeda sejenak agar pemain bisa mengagumi bidak barunya sebelum layar kembali menjauh
        yield return new WaitForSeconds(0.8f);
        
        onComplete?.Invoke();
    }
}
