using UnityEngine;
using System.Collections;
using System;

public enum PieceType
{
    Pawn,
    Rook,
    Knight,
    Bishop,
    Queen,
    King
}

public enum MoveStyle
{
    Walk,
    Jump
}

public class ChessPiece : MonoBehaviour
{
    private Animator animator;
    public PieceType pieceType;

    public bool isWhite;

    public int currentX;
    public int currentY;
    public bool hasMoved = false;
    private bool isBusy = false;
    
    public MoveStyle moveStyle = MoveStyle.Walk;
    public float moveDuration = 0.5f;
    public float jumpHeight = 1.0f;

    public void SetPosition(int x, int y)
    {
        currentX = x;
        currentY = y;
    }

    private void Awake()
    {
        // Mengambil dari Children karena biasanya Animator FBX ada di objek anak
        animator = GetComponentInChildren<Animator>();
        
        if (pieceType == PieceType.Knight || pieceType == PieceType.Bishop)
        {
            moveStyle = MoveStyle.Jump;
        }
        else
        {
            moveStyle = MoveStyle.Walk;
        }
    }

    private Quaternion defaultRotation;

    private void Start()
    {
        defaultRotation = transform.rotation;
        if (animator != null)
        {
            StartCoroutine(RandomIdleRoutine());
        }
    }

    private IEnumerator RandomIdleRoutine()
    {
        // Tunggu satu frame di awal agar Animator menyelesaikan inisialisasi internalnya
        yield return null;

        if (animator != null)
        {
            // Ambil state default (diam/idle) yang aktif saat pertama kali game mulai
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            int defaultStateHash = stateInfo.fullPathHash;
            float animLength = stateInfo.length > 0 ? stateInfo.length : 1.0f;

            // Langsung freeze bidak di frame awal setelah mendapatkan info state default
            animator.speed = 0f;

            // Jeda acak awal agar bidak tidak mulai memutar idle secara serentak
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 3f));

            while (true)
            {
                if (!isBusy && animator != null)
                {
                    animator.speed = 1f;
                    // Mainkan state default dari frame 0
                    animator.Play(defaultStateHash, 0, 0f);

                    // Tunggu durasi animasi selesai
                    yield return new WaitForSeconds(animLength);

                    // Freeze kembali setelah selesai diputar jika tidak sedang sibuk
                    if (!isBusy)
                    {
                        animator.speed = 0f;
                    }
                }

                // Tunggu jeda acak sebelum memutar kembali
                yield return new WaitForSeconds(UnityEngine.Random.Range(3f, 6f));
            }
        }
    }

    private IEnumerator FreezeAfterTransition()
    {
        // Tunggu transisi kembali ke state Idle selesai (biasanya 0.25 - 0.3 detik)
        yield return new WaitForSeconds(0.3f);
        if (!isBusy && animator != null)
        {
            animator.speed = 0f;
        }
    }

    public void MoveTo(Vector3 targetPosition, Action onComplete = null)
    {
        StartCoroutine(MoveCoroutine(targetPosition, onComplete));
    }

    private IEnumerator MoveCoroutine(Vector3 targetPosition, Action onComplete)
    {
        isBusy = true;

        if (animator != null)
        {
            animator.speed = 1f; // Pastikan kecepatan normal saat bergerak
            Debug.Log(gameObject.name + " memutar trigger Move!");
            animator.SetTrigger("Move");
        }
        else
        {
            Debug.LogWarning("Animator tidak ditemukan pada " + gameObject.name + "!");
        }

        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        if (moveStyle == MoveStyle.Jump)
        {
            if (MusicManager.Instance != null) MusicManager.Instance.PlayJump();
        }

        System.Collections.Generic.Dictionary<Transform, Quaternion> defaultChildRotations = new System.Collections.Generic.Dictionary<Transform, Quaternion>();
        foreach (Transform child in transform)
        {
            defaultChildRotations[child] = child.localRotation;
        }

        Vector3 direction = (targetPosition - startPosition).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;

            if (!isWhite)
            {
                foreach (Transform child in transform)
                {
                    child.localRotation = defaultChildRotations[child] * Quaternion.Euler(0f, 180f, 0f);
                }
            }
        }

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;

            if (moveStyle == MoveStyle.Walk)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            }
            else if (moveStyle == MoveStyle.Jump)
            {
                Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, t);
                currentPos.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;
                transform.position = currentPos;
            }

            yield return null;
        }

        transform.position = targetPosition;

        // Reset rotasi kembali ke posisi awal prefab saat pertama kali di-spawn
        transform.rotation = defaultRotation;

        if (!isWhite)
        {
            foreach (Transform child in transform)
            {
                if (defaultChildRotations.ContainsKey(child))
                {
                    child.localRotation = defaultChildRotations[child];
                }
            }
        }

        isBusy = false;

        if (animator != null)
        {
            // Memanggil Trigger "Idle" agar kembali ke animasi diam
            animator.SetTrigger("Idle");
            // Mulai proses freeze setelah transisi selesai
            StartCoroutine(FreezeAfterTransition());
        }

        onComplete?.Invoke();
    }

    public void PlayAttackAnimation(Action onComplete = null)
    {
        StartCoroutine(AttackCoroutine(onComplete));
    }

    private IEnumerator AttackCoroutine(Action onComplete)
    {
        isBusy = true;

        if (animator != null)
        {
            animator.speed = 1f; // Pastikan kecepatan normal saat menyerang
            // Mencegah konflik dengan Trigger Idle yang dipanggil di akhir gerak
            animator.ResetTrigger("Idle"); 
            
            Debug.Log(gameObject.name + " memutar trigger Attack!");
            animator.SetTrigger("Attack");
            
            yield return new WaitForSeconds(0.1f);
            
            float length = 1.0f; 
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // Jika Anda menamai state-nya dengan nama lain, abaikan pengecekan ini 
            // dan gunakan nilai default 1.0f atau sesuaikan panjangnya.
            if (stateInfo.IsName("Attack"))
            {
                length = stateInfo.length;
            }
            
            yield return new WaitForSeconds(length);
            
            isBusy = false;
            // Kembali ke Idle setelah selesai menyerang
            animator.SetTrigger("Idle");
            StartCoroutine(FreezeAfterTransition());
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            isBusy = false;
        }

        onComplete?.Invoke();
    }

    public void PlayLoseAnimation(Action onComplete = null)
    {
        StartCoroutine(LoseCoroutine(onComplete));
    }

    private IEnumerator LoseCoroutine(Action onComplete)
    {
        isBusy = true;

        if (animator != null)
        {
            animator.speed = 1f; // Pastikan kecepatan normal saat kalah
            animator.ResetTrigger("Idle");
            
            Debug.Log(gameObject.name + " memutar trigger Lose!");
            animator.SetTrigger("Lose");
            
            yield return new WaitForSeconds(0.1f);
            
            float length = 1.0f;
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Lose"))
            {
                length = stateInfo.length;
            }
            
            yield return new WaitForSeconds(length);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        onComplete?.Invoke();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (animator != null) animator.SetTrigger("Move"); 
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            PlayAttackAnimation();
        }
    }
}