// This script is attached to the camera that is going to orbit around the world


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
        bool leftLimit = orbitTarget.position.x <= 0;
        bool rightLimit = orbitTarget.position.x >= MaxMoveableDistance.x;
        bool bottomLimit = orbitTarget.position.z <= 0;
        bool topLimit = orbitTarget.position.z >= MaxMoveableDistance.y;

        bool movingLeft = Input.GetAxis("Horizontal") < 0;
        bool movingRight = Input.GetAxis("Horizontal") > 0;
        bool movingUp = Input.GetAxis("Vertical") > 0;
        bool movingDown = Input.GetAxis("Vertical") < 0;

        bool alignedPosition = Vector3.Dot(Vector3.forward, orbitTarget.forward) >= 0;//orbitTargetPosition has the z axis aligned positively

        bool localZAxisOnTheRight = (orbitTarget.forward.x - Vector3.forward.x) >= 0;//Means the blue arrow is pointing towards right relative to Vector3.forward
        bool localXOnUpSide = (orbitTarget.right.z - Vector3.right.z) >= 0;//Means red arrow of the transform is pointing up relative to Vector3.right

        Vector3 moveHorizontal = Input.GetAxis("Horizontal") * orbitTarget.right * movSpeed * Time.deltaTime;
        Vector3 moveVertical = Input.GetAxis("Vertical") * orbitTarget.forward * movSpeed * Time.deltaTime;
        Vector3 moveHorizontalOnlyHorizontalDirection = new Vector3(Input.GetAxis("Horizontal") * orbitTarget.right.x,0,0) * movSpeed * Time.deltaTime;
        Vector3 moveHorizontalOnlyVerticalDirection = new Vector3(0,0,Input.GetAxis("Horizontal") * orbitTarget.right.z) * movSpeed * Time.deltaTime;
        Vector3 moveVerticalOnlyHorizontalDirection = new Vector3(Input.GetAxis("Vertical") * orbitTarget.forward.x,0,0) * movSpeed * Time.deltaTime;
        Vector3 moveVerticalOnlyVerticalDirection = new Vector3(0,0,Input.GetAxis("Vertical") * orbitTarget.forward.z) * movSpeed * Time.deltaTime;

        bool betweenLeftAndRightLimits = orbitTarget.position.x > 0 && orbitTarget.position.x < MaxMoveableDistance.x;
        bool betweenBottomAndTopLimits = orbitTarget.position.z > 0 && orbitTarget.position.z < MaxMoveableDistance.y;
        if(betweenLeftAndRightLimits && betweenBottomAndTopLimits) {
            orbitTarget.position += Input.GetAxis("Vertical") * orbitTarget.forward * movSpeed * Time.deltaTime;
            orbitTarget.position += Input.GetAxis("Horizontal") * orbitTarget.right * movSpeed * Time.deltaTime;
        }
        /*** corner cases ***/
        else if(leftLimit && bottomLimit) {//OK
            if(alignedPosition) {
                if(movingRight) orbitTarget.position += moveHorizontalOnlyHorizontalDirection;
                if(movingUp) orbitTarget.position += moveVerticalOnlyVerticalDirection;
                if(!localZAxisOnTheRight && movingDown) orbitTarget.position += moveVerticalOnlyHorizontalDirection;
                if(!localXOnUpSide && movingLeft) orbitTarget.position += moveHorizontalOnlyVerticalDirection;
            } else {
                if(movingLeft) orbitTarget.position += moveHorizontalOnlyHorizontalDirection;
                if(movingDown) orbitTarget.position += moveVerticalOnlyVerticalDirection;
                if(localXOnUpSide && movingRight) orbitTarget.position += moveHorizontalOnlyVerticalDirection;
                if(localZAxisOnTheRight && movingUp) orbitTarget.position += moveVerticalOnlyHorizontalDirection;
            }
        }
        else if(leftLimit && topLimit) {//OK
            if(alignedPosition) {
                if(movingLeft && localXOnUpSide) orbitTarget.position += moveHorizontalOnlyVerticalDirection;
                if(movingUp && localZAxisOnTheRight) orbitTarget.position += moveVerticalOnlyHorizontalDirection;
                if(movingRight) orbitTarget.position += moveHorizontalOnlyHorizontalDirection;
                if(movingDown) orbitTarget.position += moveVerticalOnlyVerticalDirection;
            } else {
                if(movingDown && !localZAxisOnTheRight/*izq*/) orbitTarget.position += moveVerticalOnlyHorizontalDirection;
                if(movingUp) orbitTarget.position += moveVerticalOnlyVerticalDirection;
                if(movingRight && localZAxisOnTheRight) orbitTarget.position += moveHorizontalOnlyVerticalDirection;
                if(movingLeft) orbitTarget.position += moveHorizontalOnlyHorizontalDirection;
            }
        }
        else if (rightLimit && bottomLimit) {//OK
            if(alignedPosition) {
                if(movingLeft) orbitTarget.position += moveHorizontalOnlyHorizontalDirection;
                if(movingUp) orbitTarget.position += moveVerticalOnlyVerticalDirection;
                if(movingRight && localXOnUpSide) orbitTarget.position += moveHorizontalOnlyVerticalDirection;
                if(movingDown && localZAxisOnTheRight) orbitTarget.position += moveVerticalOnlyHorizontalDirection;
            } else {
                if(movingRight) orbitTarget.position += moveHorizontalOnlyHorizontalDirection;
                if(movingDown) orbitTarget.position += moveVerticalOnlyVerticalDirection;
                if(movingLeft && !localXOnUpSide) orbitTarget.position += moveHorizontalOnlyVerticalDirection;
                if(movingUp && !localZAxisOnTheRight) orbitTarget.position += moveVerticalOnlyHorizontalDirection;
            }
        }
        else if (rightLimit && topLimit) {//OK
            if(alignedPosition) {
                if(movingLeft) orbitTarget.position += moveHorizontalOnlyHorizontalDirection;
                if(movingDown) orbitTarget.position += moveVerticalOnlyVerticalDirection;
                if(movingRight && !localXOnUpSide) orbitTarget.position += moveHorizontalOnlyVerticalDirection;
                if(movingUp && !localZAxisOnTheRight) orbitTarget.position += moveVerticalOnlyHorizontalDirection;
            } else {
                if(movingLeft && localXOnUpSide) orbitTarget.position += moveHorizontalOnlyVerticalDirection;
                if(movingRight) orbitTarget.position += moveHorizontalOnlyHorizontalDirection;
                if(movingDown && localZAxisOnTheRight) orbitTarget.position += moveVerticalOnlyHorizontalDirection;
                if(movingUp) orbitTarget.position += moveVerticalOnlyVerticalDirection;
            }
        } else {
            if(leftLimit && movingLeft) {//OK
                if(alignedPosition) orbitTarget.position += moveHorizontalOnlyVerticalDirection;
                else orbitTarget.position += moveHorizontal;
            }
            else if(rightLimit && movingLeft) {//OK
                if(alignedPosition) orbitTarget.position += moveHorizontal;
                else orbitTarget.position += moveHorizontalOnlyVerticalDirection;
            }
            else if(bottomLimit && movingLeft) {//OK
                if(localXOnUpSide) orbitTarget.position += moveHorizontalOnlyHorizontalDirection;
                else orbitTarget.position += moveHorizontal;//lado abajo de Vector3.right
            }
            else if(topLimit && movingLeft) {//OK
                if(localXOnUpSide) orbitTarget.position += moveHorizontal;
                else orbitTarget.position += moveHorizontalOnlyHorizontalDirection;
            }
            if(leftLimit && movingRight){//OK
                if(alignedPosition) orbitTarget.position += moveHorizontal;
                else orbitTarget.position += moveHorizontalOnlyVerticalDirection;
            }
            else if(rightLimit && movingRight) {//OK
                if(alignedPosition) orbitTarget.position += moveHorizontalOnlyVerticalDirection;
                else orbitTarget.position += moveHorizontal;
            }
            else if (bottomLimit && movingRight) {//OK
                if(localXOnUpSide) orbitTarget.position +=  moveHorizontal;
                else orbitTarget.position += moveHorizontalOnlyHorizontalDirection;
            }
            else if(topLimit && movingRight) {//OK
                if(localXOnUpSide) orbitTarget.position += moveHorizontalOnlyHorizontalDirection;
                else orbitTarget.position += moveHorizontal;
            }

            if(leftLimit && movingDown){//OK
                if(localZAxisOnTheRight) orbitTarget.position += moveVerticalOnlyVerticalDirection;
                else orbitTarget.position += moveVertical;//direccion izquierda
            }
            else if(bottomLimit && movingDown ) {//OK
                if(alignedPosition) orbitTarget.position += moveVerticalOnlyHorizontalDirection;
                else orbitTarget.position += moveVertical;
            }
            else if(rightLimit && movingDown) {//OK
                if(localZAxisOnTheRight) orbitTarget.position += moveVertical;
                else orbitTarget.position += moveVerticalOnlyVerticalDirection;//dir izq
            }
            else if(topLimit && movingDown) {//OK
                if(alignedPosition) orbitTarget.position += moveVertical;
                else orbitTarget.position += moveVerticalOnlyHorizontalDirection;
            }

            if(leftLimit && movingUp){//Ok
                if(localZAxisOnTheRight) orbitTarget.position += moveVertical;
                else orbitTarget.position += moveVerticalOnlyVerticalDirection;
            }
            else if(rightLimit && movingUp) {//Ok
                if(localZAxisOnTheRight) orbitTarget.position += moveVerticalOnlyVerticalDirection;
                else orbitTarget.position += moveVertical;
            }
            else if(bottomLimit && movingUp) {//OK
                if(alignedPosition) orbitTarget.position += moveVertical;
                else orbitTarget.position += moveVerticalOnlyHorizontalDirection;
            }
            else if(topLimit && movingUp) {//OK
                if(alignedPosition) orbitTarget.position += moveVerticalOnlyHorizontalDirection;
                else orbitTarget.position += moveVertical;
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
