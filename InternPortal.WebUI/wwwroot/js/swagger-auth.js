(function () {
    const authContainer = `
        <div id="custom-auth-header" style="background: #3b4151; padding: 20px; border-bottom: 2px solid #49cc90; display: flex; align-items: center; gap: 15px;">
            <strong style="color: white; font-family: sans-serif;">Database Authentication:</strong>
            <input type="text" id="input-email" placeholder="Email" style="padding: 8px; border-radius: 4px; border: none; width: 200px;"/>
            <input type="password" id="input-pass" placeholder="Password" style="padding: 8px; border-radius: 4px; border: none; width: 200px;"/>
            <button id="btn-db-login" style="background: #49cc90; color: white; border: none; padding: 8px 20px; border-radius: 4px; cursor: pointer; font-weight: bold;">Login & Authorize</button>
        </div>
    `;

    window.addEventListener('load', function () {
        setTimeout(() => {
            const topbar = document.querySelector('.swagger-ui .topbar');
            if (topbar) {
                const div = document.createElement('div');
                div.innerHTML = authContainer;
                topbar.after(div);

                document.getElementById('btn-db-login').addEventListener('click', async () => {
                    const email = document.getElementById('input-email').value;
                    const password = document.getElementById('input-pass').value;

                    try {
                        const response = await fetch('/api/Users/login', {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify({ email: email, password: password })
                        });

                        if (response.ok) {
                            const result = await response.json();
                            const token = typeof result === 'string' ? result : (result.token || result.accessToken || result);

                            const ui = window.ui;
                            ui.authActions.authorize({
                                "Bearer": {
                                    name: "Bearer",
                                    schema: { type: "apiKey", in: "header", name: "Authorization", description: "" },
                                    value: token
                                }
                            });
                            alert("Success: Token has been injected.");
                        } else {
                            const errorData = await response.json();
                            alert("Failed: " + (errorData.message || "Invalid credentials"));
                        }
                    } catch (err) {
                        alert("Error: Check console for details.");
                        console.error(err);
                    }
                });
            }
        }, 1000);
    });
})();