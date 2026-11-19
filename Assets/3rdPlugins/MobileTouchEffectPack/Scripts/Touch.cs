using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using UnityEngine;
namespace MobileTouchEffectPack
{
    public class Touch : MonoBehaviour
    {
        // public Transform particle_transform;
        // public ParticleSystem particle_particle;
        public static Touch Instance;

        public UIParticle uiParticle;
        public Canvas canvas;
        public bool dragPlayMode;
        Vector3 pos_particle;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {

                MoveParticlePosition();
                // particle_particle.Play();
                //uiParticle.Play();

            }

            if (dragPlayMode)
            {
                if (Input.GetMouseButton(0))
                {

                    MoveParticlePosition();
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                //uiParticle.Stop();
                // particle_particle.Stop();
            }
        }

        public void SetUIParticleActive(bool active)
        {
            //uiParticle.gameObject.SetActive(active);
        }
        void MoveParticlePosition()
        {
            // pos_particle = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 pos = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, canvas.worldCamera, out pos);
            pos_particle = pos;

            pos_particle.z = 0;

            //uiParticle.transform.localPosition = pos_particle;
            // particle_transform.localPosition = pos_particle;
        }


    }
}