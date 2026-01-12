const apiUrl = 'http://localhost:5169/api';

document.getElementById('login-form').addEventListener('submit', async (e) => {
    e.preventDefault();

    const email = document.getElementById('email').value;
    const name = document.getElementById('name').value;

    try{
        const response = await fetch(`${apiUrl}/author/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({name:name, emailAddress:email})
        });

        if(response.ok){
            const data = await response.json();
            localStorage.setItem('token', data.user.id);
            console.log(data.user.id);
            window.location.href = 'app.html';
        }
        else{
            alert('Invalid email or name');
        }
    }
    catch(error){
        console.error('Error during login', error);
        alert('Something went wrong');
    }
});

document.getElementById('registration-button').addEventListener('click', () => {
    window.location.href = "registration.html";
})