// Funciones para manejar la persistencia de autenticación en localStorage
window.authPersistence = {
    // Guardar datos de autenticación
    saveAuth: function (token, expiration, refreshToken, userName, roles) {
        try {
            localStorage.setItem('auth_token', token);
            localStorage.setItem('auth_expiration', expiration);
            localStorage.setItem('auth_refresh_token', refreshToken || '');
            localStorage.setItem('auth_username', userName);
            localStorage.setItem('auth_roles', roles.join(','));
            return true;
        } catch (error) {
            console.error('Error guardando autenticación:', error);
            return false;
        }
    },

    // Cargar datos de autenticación
    loadAuth: function () {
        try {
            const token = localStorage.getItem('auth_token');
            const expiration = localStorage.getItem('auth_expiration');
            const refreshToken = localStorage.getItem('auth_refresh_token');
            const userName = localStorage.getItem('auth_username');
            const roles = localStorage.getItem('auth_roles');

            if (token && expiration) {
                const expDate = new Date(expiration);
                const now = new Date();
                
                console.log('authPersistence.loadAuth: Verificando expiración');
                console.log('authPersistence.loadAuth: Token expiration:', expDate.toISOString());
                console.log('authPersistence.loadAuth: Current time:', now.toISOString());
                console.log('authPersistence.loadAuth: Is valid:', expDate > now);
                
                if (expDate > now) {
                    return {
                        Token: token,
                        Expiration: expiration,
                        RefreshToken: refreshToken,
                        UserName: userName,
                        Roles: roles ? roles.split(',') : []
                    };
                } else {
                    // Token expirado, limpiar
                    console.log('authPersistence.loadAuth: Token expirado, limpiando...');
                    this.clearAuth();
                }
            }
            return null;
        } catch (error) {
            console.error('Error cargando autenticación:', error);
            return null;
        }
    },

    // Limpiar datos de autenticación
    clearAuth: function () {
        try {
            localStorage.removeItem('auth_token');
            localStorage.removeItem('auth_expiration');
            localStorage.removeItem('auth_refresh_token');
            localStorage.removeItem('auth_username');
            localStorage.removeItem('auth_roles');
            return true;
        } catch (error) {
            console.error('Error limpiando autenticación:', error);
            return false;
        }
    },

    // Verificar si hay autenticación válida
    isAuthenticated: function () {
        const auth = this.loadAuth();
        return auth !== null;
    },

    // Limpiar tokens viejos (para desarrollo)
    clearOldTokens: function () {
        console.log('authPersistence: Limpiando tokens viejos...');
        this.clearAuth();
    },

    // Limpiar solo tokens expirados
    clearExpiredTokens: function () {
        const token = localStorage.getItem("auth_token");
        const expiration = localStorage.getItem("auth_expiration");
        
        if (token && expiration) {
            const expDate = new Date(expiration);
            const now = new Date();
            
            if (expDate <= now) {
                console.log('authPersistence: Token expirado, limpiando...');
                this.clearAuth();
            } else {
                console.log('authPersistence: Token válido, manteniendo...');
            }
        }
    }
};
