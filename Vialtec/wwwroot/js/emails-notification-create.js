$(document).ready(function (e) {

    function validateEmail(email) {
        const re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        return re.test(String(email).toLowerCase());
    }

    var emailArray = [];

    $('#form1').submit(function (e) {
        if (emailArray.length === 0) {
            e.preventDefault();
            Swal.fire('Error', 'No has agregado ningún correo electrónico para el perfil de notificaciones', 'error');
        }
    });

    $('#btn-new-email').click((e) => {
        const newEmail = $('#new-email').val();
        if (newEmail !== '') {
            if (emailArray.includes(newEmail)) {
                alert('El correo ya existe en el listado');
            } else {
                // viene del archivo validar-email porque en un .cshtml no se puede usar el arroba
                if (validateEmail(newEmail)) {
                    emailArray.push(newEmail);
                    refreshEmailsTable();
                } else {
                    alert('El formato de correo no es válido');
                }
            }
            $('#new-email').val('');
        }
    });

    $('#emails-container').on('click', '.btn-delete-email', function (e) {
        const email_to_delete = $(this).attr('id');
        emailArray = emailArray.filter(x => x !== email_to_delete);
        refreshEmailsTable();
    });

    refreshEmailsTable = () => {
        $('#emails-container .btn-group').remove();

        if (emailArray.length === 0) {
            const emailSpan = `
                            <div class="btn-group btn-group-sm" role="group">
                              <button type="button" class="btn btn-light">Ninguno</button>
                            </div>
                        `;
            $('#emails-container').append(emailSpan);
        } else {
            emailArray.forEach(email => {
                const emailGroupButton = `
                            <div class="btn-group btn-group-sm mb-2" role="group">
                              <button type="button" class="btn btn-light">${ email}</button>
                              <button type="button" class="btn btn-danger btn-delete-email" id="${ email}"><i class="fas fa-trash"></i></button>
                            </div>
                        `;
                $('#emails-container').append(emailGroupButton);
            });
        }
        // asignar el valor para el input hidden de emails
        $('#EmailAddress').val(emailArray.join('#'));
    };

    // Llamada inicial
    refreshEmailsTable();
});