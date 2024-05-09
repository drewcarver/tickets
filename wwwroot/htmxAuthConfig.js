const clientID = "kRYFKmwQiKoGQegrMTTmwRpgHoRZaByz"
const domain = "dev-fu83rki86r8dd5bd.us.auth0.com"

const webAuth = new auth0.WebAuth({
  domain,
  clientID,
  audience: "Todo API",
  responseType: "token",
  redirectUri: `${window.location.origin}/authorize.html`,
})
const authToken = sessionStorage.getItem("access_token")

const login = () => {
  webAuth.authorize()
}

const logout = () => {
  sessionStorage.removeItem("access_token")
  webAuth.logout({ returnTo: "http://localhost:5000", clientID })
}

const checkSession = () =>
  new Promise((resolve, reject) =>
    webAuth.checkSession(
      { audience: "Todo API" },
      function (err, authResult) {
        if (err) reject(err)
        else resolve(authResult)
      }
    )
  )

function isTokenExpired(token) {
  const decodedToken = JSON.parse(atob(token.split(".")[1]))
  const expirationTime = decodedToken.exp * 1000 

  return Date.now() >= expirationTime
}

document.body.addEventListener("htmx:confirm", (e) => {
  const authToken = sessionStorage.getItem('access_token')

  if (!authToken ) {
    return;
  }

  if(isTokenExpired(authToken)) {
    e.preventDefault()

    webAuth.checkSession()
      .then(token => sessionStorage.setItem('access_token', token))
      .then(() => e.detail.issueRequest())
      .catch(() => login())
  }
})

document.body.addEventListener('htmx:configRequest', function(evt) {
  const authToken = sessionStorage.getItem("access_token")

  evt.detail.headers['Authorization'] = `Bearer ${authToken}` 
});