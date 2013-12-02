// This script is attached to the camera that is going to orbit around the world

//TODO: Organize the camera so it can move around the world but dont go out of the word bounds

using System.Collections;
using UnityEngine;

public class CameraOrbit : MonoBehaviour {
    
    public float xRotationMin = 0f;
    public float xRotationMax = 360f;
    public float yRotationMin  = 24.5f;
    public float yRotationMax  = 90f;
    public float maxZoomeableDistance = 20f;
    public float minZoomeableDistance = 5f;


    private Transform orbitTarget;

    private float rotationX;
    private float rotationY;
    private float rotationSpeed = 300f; //option
    private float zoomSpeed = 100f; //option
    private float movementSpeed = 500.0f;//option


    private float distanceToObject;

    // the movement goes from 0,0 to MaxMoveableDistance.x, MaxMoveableDistance.y
    public static Vector2 MaxMoveableDistance;//This gets set from EntryPoint.cs depending on the size of the world
    void Start() {
        orbitTarget = new GameObject("Camera Orbit Pivot").transform;

        orbitTarget.rotation = Quaternion.identity;
        rotationX = transform.eulerAngles.y;
        rotationY = transform.eulerAngles.x;	
        distanceToObject = 10;//(transform.position - orbitTarget.position).magnitude; 
    }
    

    private float movSpeed;
    void FixedUpdate () {
#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
        /*  Mouse Rotation */
        movSpeed = Time.deltaTime * movementSpeed;
        if (Input.GetButton("Option") && Input.GetMouseButton(1)) {
            rotationX = rotationX + Input.GetAxis("Mouse X") * Time.deltaTime * rotationSpeed;
            rotationY = rotationY - Input.GetAxis("Mouse Y") * Time.deltaTime * rotationSpeed;
        }
        if(!(distanceToObject < minZoomeableDistance && Input.GetAxis("Mouse ScrollWheel") > 0) &&
           !(distanceToObject > maxZoomeableDistance && Input.GetAxis("Mouse ScrollWheel") < 0))
            distanceToObject += Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomSpeed * -1;
	
        rotationY = ClampAngle(rotationY, yRotationMin, yRotationMax);
        transform.rotation = Quaternion.Euler(rotationY, rotationX, 0);
        orbitTarget.localRotation = Quaternion.Euler(0, rotationX, 0);
        transform.position = transform.rotation * new Vector3(0f, 0f, -distanceToObject) + orbitTarget.position;
	
        /* Camera Movement */
	
        bool limiteIzquierdo = orbitTarget.position.x <= 0;
        bool limiteDerecho = orbitTarget.position.x >= MaxMoveableDistance.x;
        bool limiteInferior = orbitTarget.position.z <= 0;
        bool limiteSuperior = orbitTarget.position.z >= MaxMoveableDistance.y;

        bool dentroLimitesIzquierdoYDerecho = orbitTarget.position.x > 0 && orbitTarget.position.x < MaxMoveableDistance.x;
        bool dentroLimitesInferiorYSuperior = orbitTarget.position.z > 0 && orbitTarget.position.z < MaxMoveableDistance.y;
	
        bool moviendoseIzq = Input.GetAxis("Horizontal") < 0;
        bool moviendoseDer = Input.GetAxis("Horizontal") > 0;
        bool moviendoseArriba = Input.GetAxis("Vertical") > 0;
        bool moviendoseAbajo = Input.GetAxis("Vertical") < 0;

        bool posicionAlineada = Vector3.Dot(Vector3.forward, orbitTarget.forward) >= 0;//orbitTargetPosition has the z axis aligned positively

        bool direccionZLadoDerecho = (orbitTarget.forward.x - Vector3.forward.x) >= 0;//significa que la flecha azul del transform esta apuntando hacia la derecha con respecto de Vector3.forward	
        bool direccionXLadoArriba = (orbitTarget.right.z - Vector3.right.z) >= 0;// significa que la flecha roja del transform esta apuntando hacia arriba con respecto a Vector3.right
	
        Vector3 moverHorizontal = Input.GetAxis("Horizontal") * orbitTarget.right * movSpeed * Time.deltaTime;
        Vector3 moverVertical = Input.GetAxis("Vertical") * orbitTarget.forward * movSpeed * Time.deltaTime;
        Vector3 moverHorizontalSoloDirHorizontal = new Vector3(Input.GetAxis("Horizontal") * orbitTarget.right.x,0,0) * movSpeed * Time.deltaTime;
        Vector3 moverHorizontalSoloDirVertical = new Vector3(0,0,Input.GetAxis("Horizontal") * orbitTarget.right.z) * movSpeed * Time.deltaTime;
        Vector3 moverVerticalSoloDirHorizontal = new Vector3(Input.GetAxis("Vertical") * orbitTarget.forward.x,0,0) * movSpeed * Time.deltaTime;
        Vector3 moverVerticalSoloDirVertical = new Vector3(0,0,Input.GetAxis("Vertical") * orbitTarget.forward.z) * movSpeed * Time.deltaTime;

        if(dentroLimitesIzquierdoYDerecho && dentroLimitesInferiorYSuperior) {
            orbitTarget.position += Input.GetAxis("Vertical") * orbitTarget.forward * movSpeed * Time.deltaTime;
            orbitTarget.position += Input.GetAxis("Horizontal") * orbitTarget.right * movSpeed * Time.deltaTime;
        } 
        /*** esquinas ***/
        else if(limiteIzquierdo && limiteInferior) {//OK
            if(posicionAlineada) {
                if(moviendoseDer) orbitTarget.position += moverHorizontalSoloDirHorizontal;
                if(moviendoseArriba) orbitTarget.position += moverVerticalSoloDirVertical;
                if(!direccionZLadoDerecho && moviendoseAbajo) orbitTarget.position += moverVerticalSoloDirHorizontal;
                if(!direccionXLadoArriba && moviendoseIzq) orbitTarget.position += moverHorizontalSoloDirVertical;
            } else {
                if(moviendoseIzq) orbitTarget.position += moverHorizontalSoloDirHorizontal;
                if(moviendoseAbajo) orbitTarget.position += moverVerticalSoloDirVertical;
                if(direccionXLadoArriba && moviendoseDer) orbitTarget.position += moverHorizontalSoloDirVertical;
                if(direccionZLadoDerecho && moviendoseArriba) orbitTarget.position += moverVerticalSoloDirHorizontal;
            }
        }
        else if(limiteIzquierdo && limiteSuperior) {//OK
            if(posicionAlineada) {
                if(moviendoseIzq && direccionXLadoArriba) orbitTarget.position += moverHorizontalSoloDirVertical;
                if(moviendoseArriba && direccionZLadoDerecho) orbitTarget.position += moverVerticalSoloDirHorizontal;
                if(moviendoseDer) orbitTarget.position += moverHorizontalSoloDirHorizontal;
                if(moviendoseAbajo) orbitTarget.position += moverVerticalSoloDirVertical;
            } else {
                if(moviendoseAbajo && !direccionZLadoDerecho/*izq*/) orbitTarget.position += moverVerticalSoloDirHorizontal;
                if(moviendoseArriba) orbitTarget.position += moverVerticalSoloDirVertical;
                if(moviendoseDer && direccionZLadoDerecho) orbitTarget.position += moverHorizontalSoloDirVertical;
                if(moviendoseIzq) orbitTarget.position += moverHorizontalSoloDirHorizontal;
            }
        }
        else if (limiteDerecho && limiteInferior) {//OK
            if(posicionAlineada) {
                if(moviendoseIzq) orbitTarget.position += moverHorizontalSoloDirHorizontal;
                if(moviendoseArriba) orbitTarget.position += moverVerticalSoloDirVertical;
                if(moviendoseDer && direccionXLadoArriba) orbitTarget.position += moverHorizontalSoloDirVertical;
                if(moviendoseAbajo && direccionZLadoDerecho) orbitTarget.position += moverVerticalSoloDirHorizontal;
            } else {
                if(moviendoseDer) orbitTarget.position += moverHorizontalSoloDirHorizontal;
                if(moviendoseAbajo) orbitTarget.position += moverVerticalSoloDirVertical;
                if(moviendoseIzq && !direccionXLadoArriba) orbitTarget.position += moverHorizontalSoloDirVertical;
                if(moviendoseArriba && !direccionZLadoDerecho) orbitTarget.position += moverVerticalSoloDirHorizontal;
            }
        }
        else if (limiteDerecho && limiteSuperior) {//OK
            if(posicionAlineada) {
                if(moviendoseIzq) orbitTarget.position += moverHorizontalSoloDirHorizontal;
                if(moviendoseAbajo) orbitTarget.position += moverVerticalSoloDirVertical;
                if(moviendoseDer && !direccionXLadoArriba) orbitTarget.position += moverHorizontalSoloDirVertical;
                if(moviendoseArriba && !direccionZLadoDerecho) orbitTarget.position += moverVerticalSoloDirHorizontal;
            } else {
                if(moviendoseIzq && direccionXLadoArriba) orbitTarget.position += moverHorizontalSoloDirVertical;
                if(moviendoseDer) orbitTarget.position += moverHorizontalSoloDirHorizontal;
                if(moviendoseAbajo && direccionZLadoDerecho) orbitTarget.position += moverVerticalSoloDirHorizontal;
                if(moviendoseArriba) orbitTarget.position += moverVerticalSoloDirVertical;
            }
        } else {
            if(limiteIzquierdo && moviendoseIzq) {//OK
                if(posicionAlineada) orbitTarget.position += moverHorizontalSoloDirVertical;
                else orbitTarget.position += moverHorizontal;
            }
            else if(limiteDerecho && moviendoseIzq) {//OK
                if(posicionAlineada) orbitTarget.position += moverHorizontal;
                else orbitTarget.position += moverHorizontalSoloDirVertical;
            }
            else if(limiteInferior && moviendoseIzq) {//OK
                if(direccionXLadoArriba) orbitTarget.position += moverHorizontalSoloDirHorizontal;
                else orbitTarget.position += moverHorizontal;//lado abajo de Vector3.right
            }
            else if(limiteSuperior && moviendoseIzq) {//OK
                if(direccionXLadoArriba) orbitTarget.position += moverHorizontal;
                else orbitTarget.position += moverHorizontalSoloDirHorizontal;
            }
	
            if(limiteIzquierdo && moviendoseDer){//OK
                if(posicionAlineada) orbitTarget.position += moverHorizontal;
                else orbitTarget.position += moverHorizontalSoloDirVertical;
            }
            else if(limiteDerecho && moviendoseDer) {//OK
                if(posicionAlineada) orbitTarget.position += moverHorizontalSoloDirVertical;
                else orbitTarget.position += moverHorizontal;
            }
            else if (limiteInferior && moviendoseDer) {//OK
                if(direccionXLadoArriba) orbitTarget.position +=  moverHorizontal;
                else orbitTarget.position += moverHorizontalSoloDirHorizontal;
            }
            else if(limiteSuperior && moviendoseDer) {//OK
                if(direccionXLadoArriba) orbitTarget.position += moverHorizontalSoloDirHorizontal;
                else orbitTarget.position += moverHorizontal;
            }
	
            if(limiteIzquierdo && moviendoseAbajo){//OK
                if(direccionZLadoDerecho) orbitTarget.position += moverVerticalSoloDirVertical;
                else orbitTarget.position += moverVertical;//direccion izquierda 
            }
            else if(limiteInferior && moviendoseAbajo ) {//OK
                if(posicionAlineada) orbitTarget.position += moverVerticalSoloDirHorizontal;
                else orbitTarget.position += moverVertical;
            }
            else if(limiteDerecho && moviendoseAbajo) {//OK
                if(direccionZLadoDerecho) orbitTarget.position += moverVertical;
                else orbitTarget.position += moverVerticalSoloDirVertical;//dir izq
            }
            else if(limiteSuperior && moviendoseAbajo) {//OK
                if(posicionAlineada) orbitTarget.position += moverVertical;
                else orbitTarget.position += moverVerticalSoloDirHorizontal;
            }

            if(limiteIzquierdo && moviendoseArriba){//Ok
                if(direccionZLadoDerecho) orbitTarget.position += moverVertical;
                else orbitTarget.position += moverVerticalSoloDirVertical;
            }	
            else if(limiteDerecho && moviendoseArriba) {//Ok
                if(direccionZLadoDerecho) orbitTarget.position += moverVerticalSoloDirVertical;
                else orbitTarget.position += moverVertical;
            }	
            else if(limiteInferior && moviendoseArriba) {//OK
                if(posicionAlineada) orbitTarget.position += moverVertical;
                else orbitTarget.position += moverVerticalSoloDirHorizontal;
            }	
            else if(limiteSuperior && moviendoseArriba) {//OK
                if(posicionAlineada) orbitTarget.position += moverVerticalSoloDirHorizontal;
                else orbitTarget.position += moverVertical;
            }
        }
#endif
    }

    private float ClampAngle (float angle, float min, float max) {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;
        return Mathf.Clamp (angle, min, max);
    }

}
