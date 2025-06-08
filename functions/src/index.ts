import * as functions from "firebase-functions";   // ← missing import

// Optional: define the response type so Unity gets IntelliSense
/*type HelloResponse = { reply: string };
*/
export const helloWorld = functions.https.onCall((_data, _ctx) => {
    return { reply: "Hello from Firebase Functions 👋" };
});
