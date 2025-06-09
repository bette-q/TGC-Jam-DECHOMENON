// functions/src/index.ts
import * as functions from "firebase-functions/v1";
import * as admin from "firebase-admin";
// modular imports
import { FieldValue } from "firebase-admin/firestore";

admin.initializeApp();
const db = admin.firestore();

export const submitCombo = functions.https.onCall(
    async (
        data: { comboNames: string[]; isGreen: boolean },
        context
    ): Promise<{ chosenIdx: number; comboNames: string[] }> => {
        const { comboNames, isGreen } = data;
        const docRef = db.collection("game").doc("sharedState");

        // simple slot logic for test: 0 for green, 1 for red
        const chosenIdx = isGreen ? 0 : 1;

        // write only that one slot, using the modular FieldValue
        await docRef.set(
            {
                [`sockets.${chosenIdx}`]: {
                    names: comboNames,
                    isGreen,
                    updatedAt: FieldValue.serverTimestamp()
                }
            },
            { merge: true }
        );

        return { chosenIdx, comboNames };
    }
);
