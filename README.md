# YSpotify SEVENET, PHAM


### Setup
Notre Projet fonctionne entièrement sur Docker, avec une image PostgreSQL; par conséquent, pour le lancer, il suffit d'installer [Docker](https://docs.docker.com/get-docker/), le démarrer et d'effectuer la commande suivante à la racine du Projet:
```shell
docker compose up
```


### API & Swagger
Le service se trouvera alors sur http://localhost:5000.

L'interface Swagger se trouve sur http://localhost:5000/api-docs, comme convenu.
Les fichiers de Configuration OpenAPI se trouvent à l'emplacement [ici](./backend/swagger/v1/).


### Tests ( non-unitaires *D:* )
L'API est populée par Migration de base de Donnée.
Un utilisateur relié au compte Spotify de Axel, "AxelSeven" existe par défaut, avec les identifiants ``TestUser`` et ``TestPassword``.
Il appartient au groupe ``TestGroup`` et en est le Chef.

Pour se Connecter/S'inscrire au service, il faut utiliser les endpoints [/api/users/login](http://localhost:5000/api/users/login) et [/api/users/register](http://localhost:5000/api/users/register) respectivement.

Ensuite, pour relier l'Utilisateur Client à Spotify, on doit utiliser [/api/spotify/login](http://localhost:5000/api/spotify/login) (après s'être authentifié avec Login).


### Endpoints Spotify
* Personalité.
	* Pour accéder à la Personalité d'un Utilisateur, il vas falloir utiliser l'endpoint [/api/users/{userId}/personality](http://localhost:5000/api/users/1/personality).

	* L'objet résultant est composé de la Valence et du Tempo Moyens des chansons Likées de l'Utilisateur ainsi que de leur propension à la danse et d'un booléen indiquant si l'utilisateur préfère l'instrumental au vocal.


* Synchronisation
	* Il suffit de s'authentifier et d'accéder à [/api/users/{userId}/personality](http://localhost:5000/api/groups/synchronize).


* Playlist de Favoris
	* Il suffit de s'authentifier et de POSTer sur l'endpoint suivant [/api/spotify/topTracksPlaylist](http://localhost:5000/api/spotify/topTracksPlaylist) avec l'ID Client de l'Utilisateur que l'on veut cibler.


