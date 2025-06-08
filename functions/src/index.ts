import * as functions from "firebase-functions/v1";
import * as admin from "firebase-admin";
admin.initializeApp();
const db = admin.firestore();

// 1️⃣ Declare the shape of each socket entry
export interface SocketState {
    prefabs: string[];
    isGreen: boolean;
    updatedAt: admin.firestore.Timestamp;
}

// 1) Define the shape of your call
export interface ComboPayload {
    roomId: string;
    socketIndex: number;
    prefabs: string[];
    isGreen: boolean;
    clientTime?: number;
}

// ③ Wire up the callable using the v1 handler signature
export const submitCombo = functions.https.onCall(
    async (
        data: ComboPayload,
        context: functions.https.CallableContext
    ): Promise<{ sockets: SocketState[] }> => {
        const { roomId, socketIndex, prefabs, isGreen, clientTime } = data;
        const timeIn = admin.firestore.Timestamp.fromMillis(clientTime ?? Date.now());
        const roomRef = db.collection("rooms").doc(roomId);

        const sockets = await db.runTransaction(async tx => {
            const snap = await tx.get(roomRef);
            const arr = (snap.exists
                ? (snap.data()!.sockets as SocketState[])
                : []
            ) as SocketState[];

            const curr = arr[socketIndex];
            if (!curr || timeIn.toMillis() > curr.updatedAt.toMillis()) {
                arr[socketIndex] = { prefabs, isGreen, updatedAt: timeIn };
                tx.set(roomRef, { sockets: arr }, { merge: true });
            }
            return arr;
        });

        return { sockets };
    }
);