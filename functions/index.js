const functions = require('firebase-functions');
const admin = require('firebase-admin');
const cors = require('cors')({ origin: true });

admin.initializeApp();
const db = admin.firestore();

exports.getSharedState = functions.https.onRequest((req, res) => {
    cors(req, res, async () => {
        const snap = await db.collection('game').doc('sharedState').get();
        res.status(200).send(snap.exists ? snap.data() : {});
    });
});

exports.setSharedState = functions.https.onRequest((req, res) => {
    cors(req, res, async () => {
        await db.collection('game').doc('sharedState')
            .set(req.body, { merge: true });
        res.status(200).send({ success: true });
    });
});
