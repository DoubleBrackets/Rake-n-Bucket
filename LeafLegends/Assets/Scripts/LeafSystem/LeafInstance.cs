using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class LeafInstance : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D rb;

    [SerializeField]
    private AnimationCurve rakeMotionCurve;

    [SerializeField]
    private float rakeMotionDuration;

    [SerializeField]
    private float distanceVariation;

    [SerializeField]
    private float rakeHorizontalVelVariation;

    [SerializeField]
    private float rakeAngleVelVariation;

    [SerializeField]
    private float defaultRakeDistance;

    [SerializeField]
    private SpriteRenderer spriteRen;

    [SerializeField]
    private Sprite[] sprites;

    private Vector2 rakeDirection;
    private float rakeDistance;

    private bool raked;
    private bool isRaking;

    private bool collected;

    public event Action Collected;

    public void ResetLeaf(float rakeAngle, float rakeDistance)
    {
        raked = false;
        collected = false;
        rb.bodyType = RigidbodyType2D.Static;
        rakeDirection = (Vector2)(Quaternion.Euler(0, 0, rakeAngle) * Vector2.up);
        this.rakeDistance = rakeDistance + Random.Range(-distanceVariation, distanceVariation);
        spriteRen.sprite = sprites[Random.Range(0, sprites.Length)];
        spriteRen.transform.localPosition = new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));
    }

    public bool TryRake()
    {
        if (isRaking)
        {
            return false;
        }

        if (!raked)
        {
            PerformRake(gameObject.GetCancellationTokenOnDestroy());
            return true;
        }
        else if (rb.GetContacts(new ContactPoint2D[1]) > 0)
        {
            PerformRake(gameObject.GetCancellationTokenOnDestroy());
            return true;
        }

        return false;
    }

    private async UniTaskVoid PerformRake(CancellationToken token)
    {
        isRaking = true;
        var time = 0f;
        Vector2 startPos = transform.position;
        rb.bodyType = RigidbodyType2D.Dynamic;
        while (time < rakeMotionDuration)
        {
            var t = time / rakeMotionDuration;
            await UniTask.Yield(PlayerLoopTiming.Update);
            if (token.IsCancellationRequested)
            {
                return;
            }

            time += Time.deltaTime;
            rb.MovePosition(startPos + rakeMotionCurve.Evaluate(t) * rakeDirection * rakeDistance);
        }

        if (!raked)
        {
            rakeDistance = defaultRakeDistance + Random.Range(-distanceVariation, distanceVariation);
            rakeDirection = Vector2.up;
            raked = true;
        }

        // Add some variation to the motion
        rb.angularVelocity = Random.Range(-rakeAngleVelVariation, rakeAngleVelVariation);
        var vel = rb.velocity;
        vel.x += Random.Range(-rakeHorizontalVelVariation, rakeHorizontalVelVariation);
        rb.velocity = vel;
        isRaking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)rakeDirection * rakeDistance);
    }

    public bool TryCollectWithBucket()
    {
        if (!raked || collected)
        {
            return false;
        }

        // can't collect leaves on the ground
        /*var contacts = new ContactPoint2D[4];
        rb.GetContacts(contacts);
        foreach (var point in contacts)
        {
            if (point.point.y < transform.position.y - 0.05f)
            {
                return false;
            }
        }*/

        collected = true;
        Collected?.Invoke();
        gameObject.SetActive(false);
        return true;
    }
}